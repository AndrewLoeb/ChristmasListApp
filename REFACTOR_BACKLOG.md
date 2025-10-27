# Refactor Backlog

This is your working list of improvements and refactoring tasks. Add, remove, and reorder items as priorities change.

---

## 🔥 High Priority

- [x] **Bulk load data from backend** - Speed up site by loading all items at once
- [x] **Add delete confirmations** - Prevent accidental item deletion (MyList.razor:157-161)
- [x] **Fix typo in method name** - `GetAllItmes` → `GetAllItems` (MyGifts.razor:34, service method)
- [x] **Add error handling** - Show user-friendly messages when Google Sheets API fails

## 📋 Medium Priority

- [x] **OpenGraph URL scraping** - Auto-populate item images from product links (Amazon requires PA-API - see Feature Enhancements)
- [x] **Add Cancel buttons** - Allow users to exit edit mode without saving (My List items & notes). Only edit one item at a time
- [x] **Add input validation** - Validate URLs, check for empty item names, character limits
- [x] **Improve mobile responsiveness** - Fix card-columns layout issues in ListReview
- [x] **Move Google Sheets credentials** - Extract hardcoded SpreadsheetId to configuration
- [x] **Remove component inheritance** - Refactor Login/MyList/ListReview/MyGifts to use proper patterns

## 🔧 Low Priority / Nice to Have
- [ ] **Manual refresh button for product metadata** - Add icon/button to re-fetch images for individual items
- [ ] **Admin page for batch product info refresh** - Update all legacy items with missing metadata
- [ ] **Manual price field** - Let users enter product prices for budget tracking

## 💡 Feature Enhancements

- [ ] **Amazon PA-API integration** - Get product images and prices from Amazon (requires Associate account with sales)
- [ ] **Manual price entry field** - Allow users to manually enter prices when auto-detection fails
- [ ] **Budget tracking** - Sum up prices, show spending totals per person
- [ ] **Search and filter** - Find items quickly in List Review
- [ ] **Item categories/tags** - Organize items by type (books, electronics, etc.)

## Won't Do (For Now)

- **Fix password security** - Hash passwords instead of plain text. Decision: Keep plain text for family app, maintain manual access via Google Sheets. May revisit later.
- **Fix date types** - Change DateUpdated/DateClaimed from string to DateTime. Decision: Data is loaded as text from Google Sheets; changing types would require data migration and risk breaking existing data. Current string format works fine for display purposes.
- **Email notifications** - Notify when items are claimed/unclaimed
- **Add pagination** - For lists with many items
- **Add dark mode** - Theme toggle support


## ✅ Completed

### Infrastructure & Architecture
- [x] **Upgrade to .NET 9.0** - Upgraded from .NET 5.0 (commit f6eee88)
- [x] **Upgrade Bootstrap 4 → 5** - Remove jQuery dependency, modernize UI (commit ebb2f8a)
- [x] **Remove component inheritance** - Refactor Login/MyList/ListReview/MyGifts to use proper patterns (commit 15c2b8c)
- [x] **Move Google Sheets credentials** - Extract hardcoded SpreadsheetId to configuration (commit ce3b34a)

### Performance & Error Handling
- [x] **Bulk load data from backend** - Speed up site by loading all items at once (commit 7e947e8)
- [x] **Add error handling** - Show user-friendly messages when Google Sheets API fails (commit 7a8f089)

### UX/UI Improvements
- [x] **Add delete confirmations** - Prevent accidental item deletion (commit 1ea7d76)
- [x] **Add Cancel buttons** - Allow users to exit edit mode without saving (commit 620e2c0)
- [x] **Add input validation** - Validate URLs, check for empty item names, character limits (commit 06004b6)
- [x] **Improve mobile responsiveness** - Fix card-columns layout issues in ListReview (commit d09b571)
- [x] **Add loading spinners** - Visual feedback during async operations (commit bd1f135)
- [x] **Improve accessibility** - Add ARIA labels, keyboard navigation support (commit bd1f135)
- [x] **UI theme and color enhancement** - Modern Christmas color scheme with CSS variables (commit accbed4)

### Product Metadata Features
- [x] **OpenGraph URL scraping** - Auto-populate item images from product links (commit ee6c781)
- [x] **Google Custom Search API integration** - Intelligent product image retrieval (commit 87102af)
- [x] **Display product images across all views** - MyList, ListReview, MyGifts (commit 03dcb43)
- [x] **Robust error handling for image search** - Progressive fallback for failed scrapes (commit 67cdc43)

### List Review Redesign
- [x] **Family-based List Review with color-coded cards** - Replace dropdown with person cards (commit d2f49ef)
- [x] **Item card claiming workflow with animations** - Smooth transitions (commit 7450565)
- [x] **Robust claim/unclaim with conflict detection** - Data consistency (commit e8f7740)
- [x] **Side-by-side List Review layout** - Person selector | item display (commit 71fb8ea)

### My Gifts Enhancements
- [x] **Claim Management in My Claimed Gifts page** - Group by family member and unclaim items (commit 09d4b8c)
- [x] **DateTime formatting with relative time** - Show "3 weeks ago" etc. (commit 09d4b8c)

### Authentication & Polish
- [x] **Remember Me Functionality** - Allow user to stay registered across sessions (commit 41e21fa)
- [x] **Modernize UI with Inter font** - Custom navigation, QOL improvements (commit 68265e7)
- [x] **QOL improvements** - Consistency and user experience polish (commit 0a7273e)

### Bug Fixes
- [x] **Fix typo in method name** - `GetAllItmes` → `GetAllItems` (commit 287f0ec)

### Documentation
- [x] Document current features and technical debt (FEATURES_AND_REQUIREMENTS.md)
- [x] Create feature requirements reference document (FEATURES_AND_REQUIREMENTS.md)
- [x] Create agent onboarding documentation (.claude/AGENT_ONBOARDING.md)
- [x] Create git workflow guide (.claude/git-workflow.md)

---

## Notes

- Reference [FEATURES_AND_REQUIREMENTS.md](FEATURES_AND_REQUIREMENTS.md) for detailed technical context
- When ready to work on an item, just say: "Let's work on [item description]"
- Feel free to reorder, add, or remove items at any time
- Mark items complete with `[x]` when done
