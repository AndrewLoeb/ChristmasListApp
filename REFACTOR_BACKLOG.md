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
- [x] **Upgrade Bootstrap 4 → 5** - Remove jQuery dependency, modernize UI
- [ ] **Add loading spinners** - Visual feedback during async operations
- [ ] **Fix date types** - Change DateUpdated/DateClaimed from string to DateTime
- [ ] **Improve accessibility** - Add ARIA labels, keyboard navigation
- [ ] **UI theme and color enhancement** - Update theme to a modern professional feel (including login screen spacing)
- [ ] **Manual refresh button for product metadata** - Add icon/button to re-fetch images for individual items
- [ ] **Admin page for batch product info refresh** - Update all legacy items with missing metadata
- [ ] **Manual price field** - Let users enter product prices for budget tracking

## 💡 Feature Enhancements

- [ ] **Amazon PA-API integration** - Get product images and prices from Amazon (requires Associate account with sales)
- [ ] **Manual price entry field** - Allow users to manually enter prices when auto-detection fails
- [ ] **Budget tracking** - Sum up prices, show spending totals per person
- [ ] **Search and filter** - Find items quickly in List Review
- [ ] **Item categories/tags** - Organize items by type (books, electronics, etc.)
- [ ] **Remember Me Functionality** - Allow a user to stay registered across sessions
- [ ] **Personal claim status in List Review** - In selection of another family member, show if the current user has claimed something for them
- [ ] **Claim Management in My Claimed Gifts page** - Group by family member and unclaim items

## Won't Do (For Now)

- **Fix password security** - Hash passwords instead of plain text. Decision: Keep plain text for family app, maintain manual access via Google Sheets. May revisit later.
- **Email notifications** - Notify when items are claimed/unclaimed
- **Add pagination** - For lists with many items
- **Add dark mode** - Theme toggle support


## ✅ Completed

- [x] Document current features and technical debt
- [x] Create feature requirements reference document

---

## Notes

- Reference [FEATURES_AND_REQUIREMENTS.md](FEATURES_AND_REQUIREMENTS.md) for detailed technical context
- When ready to work on an item, just say: "Let's work on [item description]"
- Feel free to reorder, add, or remove items at any time
- Mark items complete with `[x]` when done
