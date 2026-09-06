# Holiday Planning — Product & Technical Design

> Status: v1 — written for issue #23. This document is the spec: it replaces the
> `docs/epics/` + `docs/features/` structure for now (James: *"feel free to
> ignore the issue structure"*). It states the product framing, the MVP cut,
> the bold decisions and why, and the architecture that the first implementation
> slice (PR for #23) begins to build.

---

## 1. Product framing

**The product is not a travel search engine. It is a group decision room.**

Skyscanner, Booking and Airbnb already solve *"find me a flight/hotel"*. What
nobody solves is the part that actually kills group holidays: five people with
different budgets, dates, and tastes trying to converge on one bookable plan
inside a chaotic WhatsApp thread. Availability polls get ignored, the loudest
voice wins, the person whose dealbreaker got steamrolled quietly drops out.

So the unit of value here is a **Trip**: a shared room where a group

1. states what they each want (**preferences**) and what they each cannot
   accept (**dealbreakers**),
2. sees candidate holidays **scored against the whole group**, with an honest,
   readable explanation of who wins, who compromises, and which dealbreakers
   block what,
3. talks it through in a chat that is native to the decision (options, votes
   and vetoes appear *in* the conversation), and
4. converges on a choice.

Search and booking data feed this room; they are commodity inputs. The
defensible core is the **Compromise Engine** — the scoring/explanation layer —
and the group workflow around it.

## 2. Bold decisions

These are commitments, not suggestions. Each one deliberately rules something
out. If one turns out wrong, the design localises the damage.

### D1 — The engine is deterministic; the LLM is a narrator, never a judge
The scoring of options against group preferences is a pure, deterministic,
unit-testable function. LLMs plug in *around* it later (see §7): eliciting
preferences from chat, summarising options, proposing compromises. They never
produce the score. **Why:** a group needs to trust and interrogate the ranking
("why is Lisbon above Crete?"). A deterministic engine gives the same answer
twice and can explain itself line by line; an LLM judge can do neither.
**Rules out:** "just ask GPT to pick" MVP shortcuts.

### D2 — Explanation is a first-class output, not a debug view
Every score the engine emits carries a per-member breakdown: fit score, the
vibes that drove it, and any dealbreaker veto with a human-readable reason.
The API never returns a bare number. This directly implements James's idea
that *"options can be shown alongside which dealbreakers have to be
compromised"*. **Why:** compromise only happens when people can see exactly
what they're trading. It is also the seam where the LLM narrator plugs in.

### D3 — Fairness-aware aggregation: mean *and* least-misery
Group score = `0.6 × mean(member fits) + 0.4 × min(member fits)`. A trip that
three people love and one hates loses to a trip everyone quite likes. This is
the classic least-misery blend from group-recommender literature, and it *is*
the product's opinion about what a good group holiday is. **Rules out:** plain
averaging (tyranny of the majority) and pure least-misery (one grump vetoes
all joy). The blend constant is a named, tested constant — tunable later,
principled now.

### D4 — Dealbreakers are vetoes, not big negative weights
A dealbreaker (over budget, flight too long, missing must-have) doesn't drag a
score down — it **excludes the option for that member** and flags the option
as *blocked* with the exact reason and who holds it. Blocked options are still
returned, ranked below all viable ones, so the group can see what becomes
possible if someone relaxes a constraint. **Why:** "slightly over Anna's
budget" and "Anna mildly dislikes it" are categorically different things;
collapsing them into one number is how tools lose people's trust.

### D5 — Preferences are an open **vibe vocabulary**, weights are learned later
A preference profile is `{ vibe → weight 0..1 }` over open string vibes
(`beach`, `nightlife`, `nature`, …) with a curated seed taxonomy, **not** an
enum and **not** user-facing sliders. MVP seeds profiles directly (dummy
data / simple onboarding); the swipe-style *"pick your favourite part / your
biggest turn-off"* elicitation James sketched writes into this same structure
later, as can an LLM parsing chat. Stored as JSONB. **Why:** the elicitation
UX will change many times; the vector shape won't. Open strings mean adding a
vibe is a data change, not a schema+enum+migration change.

### D6 — Options are ephemeral; only decisions are persisted
Candidate options come from a pluggable **catalog** behind one interface
(`IOptionCatalogService`). MVP ships a static in-code catalog of ~10 realistic
destination bundles (dummy data — exactly what James asked the MVP to prove
with). Real flight/hotel APIs later implement the same interface. We do **not**
persist the catalog; we persist trips, members, preferences, and (later)
reactions/choices. **Why:** external inventory goes stale in hours; our
database should hold the things only we know — the group's state of mind.
**Rules out:** building a crawler/cache infrastructure before the concept is
proven.

### D7 — Group chat is a typed decision feed, not a text pipe
(Designed now, built next — see §6.) Messages are typed: `text`,
`option_proposed`, `reaction`, `constraint_changed`, `decision`. The chat and
the recommendation list are views over the same event stream. **Rules out:**
bolting a generic chat widget on the side and syncing it to app state later —
that path leads to two sources of truth.

### D8 — Single currency (GBP), single origin, per-person costs
MVP compares per-person GBP costs from a single implied origin (London). Multi
currency, multi-origin and whole-group cost splitting are roadmap items.
**Why:** diminishing returns — none of them change whether the core concept
lands.

## 3. MVP cut

**In (this PR + the next few):**
- Trip + members + preference profiles + dealbreakers, persisted in Postgres.
- Compromise Engine v1 over a dummy catalog: ranked options with full
  per-member explanations and veto surfacing. Unit + in-process integration
  tested.
- Minimal API: create trip, add member, get trip, get ranked recommendations.
- (next) Group chat decision feed; a plain frontend that lists ranked options
  with the explanation breakdown; reactions (favourite / turn-off) writing
  back into preference weights.

**Out (explicitly deferred):**
- Real flight/hotel/hostel/cottage data sources (catalog interface is the seam).
- Auth (gauth) — to be integrated together with James; until then the API is
  unauthenticated dev-only.
- LLM features (seams identified in §7).
- Availability/calendar overlap, wishlist price tracking, trending/bargain
  pages, swipe UI, "start from a similar past trip" — backlog, all compatible
  with this domain model.
- Aesthetics.

## 4. Domain model

```
Trip 1──* TripMember
                │ DisplayName
                │ VibeWeights        jsonb  { "beach": 0.9, "nightlife": 0.4, ... }
                │ Dealbreakers       jsonb  [ { type, value } ]

HolidayOption (not persisted — from IOptionCatalogService)
                │ Name, Country, Description
                │ VibeIntensities    { "beach": 1.0, "culture": 0.6, ... }
                │ CostPerPersonGbp, TravelHours, Nights

OptionRecommendation (computed)
                │ Option, GroupScore, IsBlocked
                └──* MemberFit { MemberName, Score, IsVetoed, Reasons[] }

(next slice)
TripMessage     │ typed decision-feed events (see §6)
Reaction        │ member × option → favourite part / biggest turn-off
```

Dealbreaker types v1 (closed set of string constants, open for extension):
- `budget_per_person_above` — vetoes options costing more than `value` GBP.
- `travel_hours_above` — vetoes options with travel longer than `value` hours.
- `must_have_vibe` — vetoes options where vibe `value` has intensity < 0.5.

Layer mapping follows the template architecture exactly
(`docs/specs/backend-architecture.md`): `*Entity` (EF) → `Domain*` (business)
→ plain-noun DataModels (API), interfaces in Abstractions, AutoMapper at both
boundaries.

## 5. The Compromise Engine (v1)

Pure function: `(option, members[]) → OptionRecommendation`.

Per member:
1. **Dealbreaker gate.** First failing dealbreaker ⇒ `IsVetoed = true`,
   fit = 0, reason recorded (e.g. *"£1,450pp exceeds Anna's £1,200 limit"*).
   The option as a whole becomes `IsBlocked`.
2. **Vibe match** ∈ [0,1]: weighted average of the option's vibe intensities
   under the member's weights —
   `Σ weight(v)·intensity(v) / Σ weight(v)`. Vibes the member doesn't weight
   are ignored; a member with no weights is neutral (0.5).
3. **Budget comfort** ∈ [0,1]: `(budget − cost) / budget`, clamped — headroom
   feels good, scraping the ceiling doesn't. Members without a budget
   dealbreaker are neutral (0.5).
4. **Member fit** = `0.7 × vibeMatch + 0.3 × budgetComfort`, with the top
   contributing vibes recorded as reasons (*"strong match: beach, food"*).

Group:
- **GroupScore** = `0.6 × mean + 0.4 × min` over member fits (D3).
- Ranking: viable options by GroupScore desc, then **blocked options** by
  GroupScore desc — visible, explained, never silently dropped (D4).

All four constants (`0.7/0.3`, `0.6/0.4`) live in one place in
`OptionScoringService` and are pinned by unit tests. The in-process
integration test ranks the full dummy catalog for a fixed two-persona group
and asserts the ordering — a golden test that makes any future tuning of the
engine a conscious, reviewed act.

## 6. Group chat: the decision feed (design — next slice)

One persistent stream per trip. Every entry is a typed event:

| type | payload | rendered as |
|---|---|---|
| `text` | member, body | normal chat bubble |
| `option_proposed` | option snapshot | option card inline in chat |
| `reaction` | member, option, favourite-part / turn-off vibe | "Anna ❤️ the beach in Crete" |
| `constraint_changed` | member, dealbreaker diff | "Ben raised his budget to £1,400 — 3 new options unlocked" |
| `decision` | option, decided-by | pinned outcome |

Consequences: the chat *is* the audit log of the decision; reactions raised in
chat are the same rows the preference-learning loop (D5) reads; and
`constraint_changed` events let the engine announce newly-unlocked options —
the compromise loop made visible. Implementation: a `TripMessage` table
(jsonb payload), plain polling first, SignalR when it hurts.

## 7. Where LLMs plug in (later, all behind existing seams)

1. **Elicitor** — parse free-text chat ("somewhere hot but not clubby") into
   vibe-weight suggestions the member confirms. Writes D5 structures.
2. **Narrator** — turn a `MemberFit[]` breakdown into a paragraph a human wants
   to read. Reads D2 structures.
3. **Mediator** — given a blocked-but-high-scoring option, draft the
   compromise message ("if Ben stretches £100, everyone's top pick unlocks").
   Reads D4 veto output, writes a `text` event.

None of these change the engine, the schema, or the API shape — that is what
D1 buys.

## 8. Roadmap after MVP

1. Chat decision feed + reactions → preference learning loop closes.
2. Frontend comparison UI (ranked cards + explanation drawer), swipe triage.
3. gauth (with James), member identity becomes real.
4. First real data source behind `IOptionCatalogService` (a free flights API),
   options become composable bundles (stay + travel).
5. Availability/date-window intersection; wishlist price tracking; trending.

## 9. This PR's slice (issue #23)

- This document.
- Domain core: `TripEntity`/`TripMemberEntity` + DbContext + initial
  migration; Trip/TripMember/HolidayOption/OptionRecommendation/MemberFit
  models across all four model layers per the template rules.
- Services: `TripStore` (EF persistence boundary), `TripService`,
  `OptionCatalogService` (dummy catalog), `OptionScoringService` (the engine),
  `RecommendationService` (orchestrator).
- Routes: `POST /api/trips`, `GET /api/trips/{id}`,
  `POST /api/trips/{id}/members`, `GET /api/trips/{id}/recommendations`.
- Tests: unit tests for the engine and orchestrators (xUnit + Moq), plus the
  in-process integration golden-ranking test. No test DB (per
  `docs/specs/testing-strategy.md`).
