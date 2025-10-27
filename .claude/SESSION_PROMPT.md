# Session Prompts for Claude Code

Use these prompts at the start of new Claude Code sessions to provide optimal context with minimal token usage.

---

## Quick Reference

| Prompt Type | Use When | Token Cost | Files Read |
|-------------|----------|------------|------------|
| **Minimal** | Simple tasks, bug fixes, typos | ~50 tokens | 1 file |
| **Standard** | Most tasks, new features | ~500 tokens | 3 files |
| **Comprehensive** | Complex features, architecture changes | ~1500 tokens | 4 files |

---

## Option 1: Minimal Prompt (Quick Tasks)

**Use for:** Typo fixes, simple bug fixes, one-file changes

```
Start by reading .claude/README.md for project overview, then proceed with the task.
```

**Example usage:**
> "Start by reading .claude/README.md for project overview, then fix the typo in MyList.razor line 45."

---

## Option 2: Standard Prompt (Recommended)

**Use for:** Most tasks, new features, refactoring, UI improvements

```
This is the Christmas List Management App. Before starting:
1. Read .claude/README.md for project overview
2. Read .claude/AGENT_ONBOARDING.md for architecture and patterns
3. Check REFACTOR_BACKLOG.md for current priorities

Then proceed with the task following patterns in git-workflow.md.
```

**Example usage:**
> "This is the Christmas List Management App. Before starting:
> 1. Read .claude/README.md for project overview
> 2. Read .claude/AGENT_ONBOARDING.md for architecture and patterns
> 3. Check REFACTOR_BACKLOG.md for current priorities
>
> Then proceed with the task following patterns in git-workflow.md.
>
> Task: Add a manual refresh button for product metadata on each item in My List."

---

## Option 3: Comprehensive Prompt (Complex Work)

**Use for:** Major features, architectural changes, first-time complex tasks

```
Christmas List Management App - Family gift list manager (.NET 9 Blazor, Google Sheets backend)

Setup:
1. Read .claude/README.md (quick overview)
2. Read .claude/AGENT_ONBOARDING.md (architecture, patterns, decisions)
3. Review REFACTOR_BACKLOG.md (current work priorities)
4. Reference FEATURES_AND_REQUIREMENTS.md if needed (detailed feature catalog)
5. Follow .claude/git-workflow.md for commits

Key context: This is a family project (not enterprise). Pragmatic decisions over patterns. Focus on UX and polish.

Now proceed with: [your task here]
```

**Example usage:**
> "Christmas List Management App - Family gift list manager (.NET 9 Blazor, Google Sheets backend)
>
> Setup:
> 1. Read .claude/README.md (quick overview)
> 2. Read .claude/AGENT_ONBOARDING.md (architecture, patterns, decisions)
> 3. Review REFACTOR_BACKLOG.md (current work priorities)
> 4. Reference FEATURES_AND_REQUIREMENTS.md if needed (detailed feature catalog)
> 5. Follow .claude/git-workflow.md for commits
>
> Key context: This is a family project (not enterprise). Pragmatic decisions over patterns. Focus on UX and polish.
>
> Now proceed with: Implement budget tracking across all views with manual price entry."

---

## Decision Guide

### Use Minimal When:
- Fixing typos or obvious bugs
- Making trivial one-line changes
- You're certain the agent doesn't need broader context
- Quick polish or formatting changes

### Use Standard When (RECOMMENDED DEFAULT):
- Adding new features from REFACTOR_BACKLOG.md
- Refactoring existing code
- UI/UX improvements
- Working with multiple files
- Any task requiring understanding of project patterns
- 90% of your sessions should use this

### Use Comprehensive When:
- Implementing major new features (budget tracking, notifications, etc.)
- Making architectural changes
- First time working on a complex subsystem
- Need detailed user story context from FEATURES_AND_REQUIREMENTS.md
- Integrating new external APIs or services

---

## Tips for Effective Sessions

1. **Be Specific**: After the setup prompt, clearly state the task
   - ✅ Good: "Add a refresh button next to each item's image in MyList.razor"
   - ❌ Bad: "Improve the product metadata feature"

2. **Reference Backlog**: Point to specific backlog items when applicable
   - "Let's work on the 'Manual refresh button for product metadata' item from REFACTOR_BACKLOG.md"

3. **Provide Context**: If the task relates to previous work, mention it
   - "Building on the Google Image Search integration (commit 87102af), add a manual refresh button..."

4. **Use TodoWrite**: For multi-step tasks, Claude will use TodoWrite to track progress

5. **Commit Workflow**: Claude will ask before committing completed features (per git-workflow.md)

---

## Example Complete Session Start

**Standard Task:**
```
This is the Christmas List Management App. Before starting:
1. Read .claude/README.md for project overview
2. Read .claude/AGENT_ONBOARDING.md for architecture and patterns
3. Check REFACTOR_BACKLOG.md for current priorities

Then proceed with the task following patterns in git-workflow.md.

Task: Add a manual refresh button for product metadata in My List. Each item should have a small icon button next to the image that, when clicked, re-fetches the OpenGraph metadata and Google Image Search results for that item's link. Use the existing ProductMetadataService.
```

---

## Troubleshooting

**If agent seems confused about architecture:**
- Add: "Review the Service Layer section in AGENT_ONBOARDING.md"

**If agent suggests something in "Won't Do" list:**
- Add: "Check the 'Won't Do (For Now)' section in REFACTOR_BACKLOG.md before suggesting alternatives"

**If agent doesn't follow commit conventions:**
- Add: "Follow the commit format exactly as specified in .claude/git-workflow.md"

**If token usage is too high:**
- Use Minimal prompt and only reference specific sections
- Example: "Read .claude/README.md, then review the ProductMetadataService section in AGENT_ONBOARDING.md"

---

## Copy-Paste Templates

### Template 1: Standard (Most Common)
```
This is the Christmas List Management App. Before starting:
1. Read .claude/README.md for project overview
2. Read .claude/AGENT_ONBOARDING.md for architecture and patterns
3. Check REFACTOR_BACKLOG.md for current priorities

Then proceed with the task following patterns in git-workflow.md.

Task: [DESCRIBE YOUR TASK HERE]
```

### Template 2: Backlog Item
```
This is the Christmas List Management App. Before starting:
1. Read .claude/README.md for project overview
2. Read .claude/AGENT_ONBOARDING.md for architecture and patterns
3. Check REFACTOR_BACKLOG.md for current priorities

Then proceed with the task following patterns in git-workflow.md.

Let's work on: [BACKLOG ITEM NAME] from REFACTOR_BACKLOG.md
```

### Template 3: Quick Fix
```
Start by reading .claude/README.md for project overview, then [DESCRIBE QUICK TASK].
```

---

## Notes

- These prompts are optimized for the documentation structure created in commit ea1de4d
- Progressive context loading: Start minimal, reference more docs only if needed
- The agent will naturally ask clarifying questions if it needs more context
- Update this file if documentation structure changes significantly

---

**Last Updated:** 2025-10-27 (commit ea1de4d)
