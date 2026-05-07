# SlowTox Visualized Landing Structure (Draft)

This document defines the target structure for the ModDB landing page before HTML implementation.

## 1) Landing Goals

- Explain value in 5-10 seconds.
- Prevent common install/usage confusion.
- Make key constraints impossible to miss (required dependencies).
- Keep content easy to scan on desktop and mobile.

## 2) Primary Audience

- **Players**: want to know what the mod changes and if it is worth installing.
- **Server pack maintainers**: need dependency clarity and compatibility confidence.
- **Modders (secondary)**: may want high-level integration context, but not deep API docs on landing.

## 3) Core User Questions (must be answered on page)

1. What does this mod do for me?
2. Is it standalone?
3. What dependencies are mandatory?
4. Why do I not see statuses sometimes?
5. Can I customize the layout?
6. What should I check if it "doesn't work"?

## 4) Information Priority (top to bottom)

1. **Hero value statement** (1 short sentence).
2. **Hard dependency warning** (`SlowTox` + `Player Status HUD` required).
3. **Why install** (3 short bullets).
4. **Visual proof** (main screenshot block).
5. **Compact FAQ** (4-5 high-value Q/A).
6. **Ultra-short troubleshooting** (3 checks max).

## 5) UX Rules for the Page

- Keep paragraph length short (1-2 lines typical).
- Prefer bullets over prose for scan speed.
- Use clear visual separation for critical warnings.
- Avoid duplicated info between sections.
- One screen should communicate value + dependencies without scrolling too much.
- FAQ should be visually distinct (accordion or clearly separated Q/A cards).

## 6) Content Boundaries

- Include:
  - Gameplay value
  - Required dependencies
  - Basic behavior notes (e.g., intoxication shown only when > 0)
  - Config reload hint (F9) in one short place
- Exclude from landing:
  - Deep architecture explanations
  - Extended dev API details
  - Changelog-level technical history

## 7) Proposed Section Skeleton

1. `Header`
   - Name: `SlowTox Visualized`
   - One-line value proposition
2. `Required Dependencies` (high-contrast warning box)
3. `Why install` (3 bullets)
4. `In-game preview` (1 placeholder image + caption)
5. `FAQ` (accordion, 4-5 items)
6. `Troubleshooting` (3 quick checks)

## 8) Placeholder Image Requirements

- **Main screenshot**:
  - Alt: `Status strip with intoxication and active SlowTox effects`
  - Goal: show readability and real usage.
- **Optional secondary screenshot**:
  - Alt: `Low intoxication vs high intoxication comparison`
  - Goal: show state change clarity.

## 9) Copy Style Guide

- Tone: practical, direct, player-first.
- Sentences: short, active voice.
- Avoid generic filler ("modern", "powerful", "immersive").
- Every section should answer a concrete user question.

## 10) Implementation Checklist (next step)

- [ ] Convert this structure into final `docs/mod-landing.html`.
- [ ] Keep total length compact (target: ~45-65 lines of meaningful content, excluding style).
- [ ] Verify dependency warning is visible near top.
- [ ] Verify no duplicate install boilerplate.
- [ ] Keep image placeholders with informative `alt`.
