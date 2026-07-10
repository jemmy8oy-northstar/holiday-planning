using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.DataModels.Models;
using HolidayPlanning.DomainModels.Models;

namespace HolidayPlanning.Services;

/// <summary>
/// The Compromise Engine (docs/DESIGN.md §5). Pure and deterministic: scores one
/// option against every member of a group and returns a full, human-readable
/// per-member explanation. No I/O, no LLM (D1). Every tuning constant lives here
/// and is pinned by unit tests, so any future tuning is a conscious, reviewed act.
/// </summary>
public class OptionScoringService : IOptionScoringService
{
    // Member fit = VibeWeight·vibeMatch + BudgetWeight·budgetComfort (DESIGN §5 step 4).
    internal const double VibeWeight = 0.7;
    internal const double BudgetWeight = 0.3;

    // Group score = MeanWeight·mean + MinWeight·min over member fits (DESIGN §2 D3).
    internal const double MeanWeight = 0.6;
    internal const double MinWeight = 0.4;

    // A member with no stated vibes / no budget dealbreaker is treated as neutral.
    internal const double NeutralScore = 0.5;

    // must_have_vibe is satisfied only at or above this intensity (DESIGN §4).
    internal const double MustHaveVibeMinIntensity = 0.5;

    // How many top-contributing vibes to name in a member's explanation.
    private const int TopVibeReasons = 2;

    public IDomainOptionRecommendation ScoreOption(
        IDomainHolidayOption option,
        IReadOnlyList<IDomainTripMember> members)
    {
        var fits = new List<MemberFit>(members.Count);
        var blocked = false;

        foreach (var member in members)
        {
            var fit = ScoreMember(option, member);
            if (fit.IsVetoed) blocked = true;
            fits.Add(fit);
        }

        return new DomainOptionRecommendation
        {
            Option = ToDataOption(option),
            MemberFits = fits,
            IsBlocked = blocked,
            GroupScore = GroupScore(fits)
        };
    }

    private static MemberFit ScoreMember(IDomainHolidayOption option, IDomainTripMember member)
    {
        // 1. Dealbreaker gate — the first failing dealbreaker vetoes the option (D4).
        var veto = FirstVeto(option, member);
        if (veto is not null)
        {
            return new MemberFit
            {
                MemberName = member.DisplayName,
                Score = 0d,
                IsVetoed = true,
                Reasons = new List<string> { veto }
            };
        }

        // 2. Vibe match, 3. budget comfort, 4. weighted member fit.
        var budget = BudgetOf(member);
        var (vibeMatch, topVibes) = VibeMatch(option, member);
        var budgetComfort = BudgetComfort(option, budget);
        var score = (VibeWeight * vibeMatch) + (BudgetWeight * budgetComfort);

        // Every fit carries a human-readable explanation (DESIGN §2 D2).
        var reasons = new List<string>();
        if (topVibes.Count > 0)
            reasons.Add($"strong match: {string.Join(", ", topVibes)}");
        else if (!member.HasVibePreferences)
            reasons.Add("no stated preferences — neutral fit");
        else
            reasons.Add("little here matches their vibes");

        if (budget is > 0d)
        {
            reasons.Add(budgetComfort >= 0.25d
                ? $"comfortably within {member.DisplayName}'s budget"
                : $"close to {member.DisplayName}'s budget ceiling");
        }

        return new MemberFit
        {
            MemberName = member.DisplayName,
            Score = score,
            IsVetoed = false,
            Reasons = reasons
        };
    }

    /// <summary>Returns the reason for the first failing dealbreaker, or null when none fail.</summary>
    private static string? FirstVeto(IDomainHolidayOption option, IDomainTripMember member)
    {
        foreach (var dealbreaker in member.Dealbreakers)
        {
            switch (dealbreaker.Type)
            {
                case DealbreakerTypes.BudgetPerPersonAbove:
                    if ((double)option.CostPerPersonGbp > dealbreaker.Threshold)
                        return $"£{option.CostPerPersonGbp:0}pp exceeds {member.DisplayName}'s £{dealbreaker.Threshold:0} budget";
                    break;

                case DealbreakerTypes.TravelHoursAbove:
                    if (option.TravelHours > dealbreaker.Threshold)
                        return $"{option.TravelHours:0.#}h travel exceeds {member.DisplayName}'s {dealbreaker.Threshold:0.#}h limit";
                    break;

                case DealbreakerTypes.MustHaveVibe:
                    if (dealbreaker.Vibe is not null && option.IntensityOf(dealbreaker.Vibe) < MustHaveVibeMinIntensity)
                        return $"{member.DisplayName} needs {dealbreaker.Vibe}; {option.Name} offers little";
                    break;

                // Unknown dealbreaker types are ignored — the set is open (DESIGN §4).
            }
        }

        return null;
    }

    /// <summary>
    /// Weighted average of the option's vibe intensities under the member's weights:
    /// Σ weight(v)·intensity(v) / Σ weight(v). A member with no weights is neutral.
    /// </summary>
    private static (double match, IReadOnlyList<string> topVibes) VibeMatch(
        IDomainHolidayOption option, IDomainTripMember member)
    {
        if (!member.HasVibePreferences)
            return (NeutralScore, Array.Empty<string>());

        double weightedSum = 0d, weightTotal = 0d;
        var contributions = new List<(string Vibe, double Contribution)>();

        foreach (var (vibe, weight) in member.VibeWeights)
        {
            if (weight <= 0d) continue;
            var intensity = option.IntensityOf(vibe);
            weightedSum += weight * intensity;
            weightTotal += weight;
            contributions.Add((vibe, weight * intensity));
        }

        if (weightTotal <= 0d)
            return (NeutralScore, Array.Empty<string>());

        var topVibes = contributions
            .Where(c => c.Contribution > 0d)
            .OrderByDescending(c => c.Contribution)
            .Take(TopVibeReasons)
            .Select(c => c.Vibe)
            .ToList();

        return (weightedSum / weightTotal, topVibes);
    }

    /// <summary>The member's budget-per-person limit, or null when they hold no budget dealbreaker.</summary>
    private static double? BudgetOf(IDomainTripMember member)
    {
        foreach (var dealbreaker in member.Dealbreakers)
        {
            if (dealbreaker.Type == DealbreakerTypes.BudgetPerPersonAbove)
                return dealbreaker.Threshold;
        }

        return null;
    }

    /// <summary>
    /// Headroom against the member's budget: (budget − cost) / budget, clamped to
    /// [0,1]. Members without a budget dealbreaker are neutral.
    /// </summary>
    private static double BudgetComfort(IDomainHolidayOption option, double? budget)
    {
        if (budget is null or <= 0d)
            return NeutralScore;

        var comfort = (budget.Value - (double)option.CostPerPersonGbp) / budget.Value;
        return Math.Clamp(comfort, 0d, 1d);
    }

    private static double GroupScore(IReadOnlyList<MemberFit> fits)
    {
        if (fits.Count == 0) return 0d;

        double sum = 0d, min = double.MaxValue;
        foreach (var fit in fits)
        {
            sum += fit.Score;
            if (fit.Score < min) min = fit.Score;
        }

        var mean = sum / fits.Count;
        return (MeanWeight * mean) + (MinWeight * min);
    }

    /// <summary>Projects the domain option to a plain data option for the recommendation.</summary>
    private static HolidayOption ToDataOption(IDomainHolidayOption option) => new()
    {
        Slug = option.Slug,
        Name = option.Name,
        Country = option.Country,
        Description = option.Description,
        CostPerPersonGbp = option.CostPerPersonGbp,
        TravelHours = option.TravelHours,
        Nights = option.Nights,
        VibeIntensities = option.VibeIntensities.ToDictionary(kv => kv.Key, kv => kv.Value)
    };
}
