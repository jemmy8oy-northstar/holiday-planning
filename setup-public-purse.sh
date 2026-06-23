#!/usr/bin/env bash
# setup-public-purse.sh
#
# Scaffolds the public-purse repo from this web-template.
# Run from the root of the holiday-planning repo.
#
# Prerequisites:
#   - gh CLI installed and authenticated (gh auth login)
#   - Write access to jemmy8oy-northstar/public-purse
#
# Usage:
#   bash setup-public-purse.sh

set -e

TEMPLATE_DIR="$(cd "$(dirname "$0")" && pwd)"
SCAFFOLD_DIR="/tmp/public-purse-scaffold"
TARGET_REPO="jemmy8oy-northstar/public-purse"

echo "[setup] Scaffolding public-purse from web-template..."

# 1. Create scaffold directory
rm -rf "$SCAFFOLD_DIR"
mkdir -p "$SCAFFOLD_DIR"
rsync -a --exclude='.git' --exclude='node_modules' --exclude='bin' --exclude='obj' \
  "$TEMPLATE_DIR/" "$SCAFFOLD_DIR/"
echo "[setup] Files copied."

cd "$SCAFFOLD_DIR"

# 2. Rename directories
mv backend/HolidayPlanning.Abstractions   backend/PublicPurse.Abstractions
mv backend/HolidayPlanning.DataModels     backend/PublicPurse.DataModels
mv backend/HolidayPlanning.Database       backend/PublicPurse.Database
mv backend/HolidayPlanning.DomainModels   backend/PublicPurse.DomainModels
mv backend/HolidayPlanning.EntityModels   backend/PublicPurse.EntityModels
mv backend/HolidayPlanning.Services       backend/PublicPurse.Services
mv backend/HolidayPlanning.WebApi         backend/PublicPurse.WebApi
mv backend/HolidayPlanning.slnx           backend/PublicPurse.slnx

# 3. Rename files
mv backend/PublicPurse.Abstractions/HolidayPlanning.Abstractions.csproj \
   backend/PublicPurse.Abstractions/PublicPurse.Abstractions.csproj
mv backend/PublicPurse.DataModels/HolidayPlanning.DataModels.csproj \
   backend/PublicPurse.DataModels/PublicPurse.DataModels.csproj
mv backend/PublicPurse.Database/HolidayPlanning.Database.csproj \
   backend/PublicPurse.Database/PublicPurse.Database.csproj
mv backend/PublicPurse.Database/HolidayPlanning.DbContext.cs \
   backend/PublicPurse.Database/PublicPurse.DbContext.cs
mv backend/PublicPurse.DomainModels/HolidayPlanning.DomainModels.csproj \
   backend/PublicPurse.DomainModels/PublicPurse.DomainModels.csproj
mv backend/PublicPurse.EntityModels/HolidayPlanning.EntityModels.csproj \
   backend/PublicPurse.EntityModels/PublicPurse.EntityModels.csproj
mv backend/PublicPurse.Services/HolidayPlanning.Services.csproj \
   backend/PublicPurse.Services/PublicPurse.Services.csproj
mv backend/PublicPurse.WebApi/HolidayPlanning.WebApi.csproj \
   backend/PublicPurse.WebApi/PublicPurse.WebApi.csproj
mv backend/PublicPurse.WebApi/HolidayPlanning.Api.http \
   backend/PublicPurse.WebApi/PublicPurse.Api.http
echo "[setup] File/directory renames complete."

# 4. Text content replacements
grep -rl 'HolidayPlanning\|holiday-planning\|holiday_planning\|indie_dev_home\|web-template-frontend' . 2>/dev/null | while IFS= read -r f; do
  if grep -qP '\x00' "$f" 2>/dev/null; then continue; fi
  sed -i \
    -e 's/HolidayPlanning/PublicPurse/g' \
    -e 's/holiday-planning/public-purse/g' \
    -e 's/holiday_planning/public_purse/g' \
    -e 's/indie_dev_home/public_purse/g' \
    -e 's/web-template-frontend/public-purse-frontend/g' \
    "$f"
done

# Update CLAUDE.md title
sed -i 's/# CLAUDE.md — web-template/# CLAUDE.md — public-purse/' CLAUDE.md
echo "[setup] Text replacements complete."

# Remove this setup script from the scaffold
rm -f setup-public-purse.sh

# 5. Git init and push
GH_TOKEN=$(gh auth token)
git init
git config user.email "$(gh api /user --jq .email 2>/dev/null || echo 'user@example.com')"
git config user.name "$(gh api /user --jq .name 2>/dev/null || echo 'Developer')"
git remote add origin "https://x-access-token:${GH_TOKEN}@github.com/${TARGET_REPO}.git"
git checkout -b main
git add .
git commit -m "feat: scaffold public-purse from web-template"
git push -u origin main
echo "[setup] Pushed to main."

# Create dev branch
git checkout -b dev
git push -u origin dev
echo "[setup] Pushed dev branch."

# 6. Scaffold SDD issues
echo "[setup] Creating initial SDD issues on ${TARGET_REPO}..."
node scripts/init-issues.mjs
echo "[setup] Done! public-purse is ready."
echo ""
echo "Next steps:"
echo "  1. Configure branch protection on main and dev (issue [1b])"
echo "  2. Add pipeline secrets (issue [1a])"
echo "  3. Fill in the [1c] spec questionnaire and tag @claude"
