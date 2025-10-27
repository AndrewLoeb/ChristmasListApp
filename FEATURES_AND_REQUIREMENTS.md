# Christmas List App - Feature Documentation & Requirements

## Purpose
This document catalogs all features, user stories, technical requirements, and architectural considerations for the Christmas List Registry application to support refactoring and improvements.

---

## User Stories

### Authentication & Access Control

#### US-001: User Login
**As a** family member
**I want to** log in with my name and password
**So that** I can access my personal list and view family members' lists

**Acceptance Criteria:**
- Dropdown displays all registered family members
- Password field validates against stored password
- Successful login displays welcome message with user's name
- Failed login shows error message "Check Password."
- Empty password shows "Enter Password" message
- Login state persists during session

**Current Implementation:**
- Component: `Login.razor`
- Service: `googleSheetsListService.Users_GetList()`
- Data: Users sheet (ID, Family, Name, Password, Notes)
- Validation: Client-side password comparison (line 87)

**Recent Improvements:**
- ✅ "Remember Me" functionality implemented (commit 41e21fa)
- ✅ LocalStorageService added for persistent login
- ✅ Auto-focus password field
- ✅ Bulk data loading at login (90% API call reduction)
- ✅ Comprehensive error handling with retry logic

**Technical Debt:**
- Passwords stored in plain text in Google Sheets (intentional - family app)
- No password hashing or encryption (deferred - see "Won't Do" in REFACTOR_BACKLOG.md)
- No session timeout mechanism

---

### Personal List Management (My List Tab)

#### US-002: View My Gift List
**As a** logged-in user
**I want to** view all items on my wish list
**So that** I can see what I've requested

**Acceptance Criteria:**
- Display table with columns: Actions, Item, Notes, Link, Date Added
- Show only my active items (Active = 1)
- Items sorted appropriately
- Links are clickable and open in new tab
- Product images displayed automatically

**Current Implementation:**
- Component: `MyList.razor`
- Service: `googleSheetsListService.GetMyList(userId)`
- Filter: Active items only
- Product images: Auto-fetched via ProductMetadataService (commit 03dcb43)

---

#### US-003: Add Gift Item
**As a** logged-in user
**I want to** add a new item to my wish list
**So that** family members know what I'd like

**Acceptance Criteria:**
- Item name is required
- Notes are optional
- Link is optional
- Date added is automatically recorded
- Success toast notification displays
- Form clears after successful add
- Item appears immediately in table

**Current Implementation:**
- Component: `MyList.razor:129-146`
- Service: `googleSheetsListService.AddItem(userId, item, notes, link)`
- Auto-increment ItemId
- Auto-set DateUpdated to current timestamp

**Recent Improvements:**
- ✅ Input validation implemented (commit 06004b6)
- ✅ URL validation for links
- ✅ Required field validation
- ✅ Character limits enforced
- ✅ Product metadata auto-fetched (OpenGraph + Google Image Search)
- ✅ Enter key support for all form fields

**Technical Considerations:**
- No validation for duplicate items (intentional - users may want similar items)

---

#### US-004: Edit Gift Item
**As a** logged-in user
**I want to** modify details of an item on my list
**So that** I can update information or correct mistakes

**Acceptance Criteria:**
- Click "Edit" button to enter edit mode
- Inline form appears with current values
- Can modify Item, Notes, and Link
- Click "Save" to commit changes
- Success toast notification displays
- DateUpdated automatically updates
- Returns to view mode after save

**Current Implementation:**
- Component: `MyList.razor:147-156`
- Service: `googleSheetsListService.UpdateItem(itemId, item, notes, link)`
- Inline editing with conditional rendering (lines 36-45)
- Uses `editItemId` state to track which item is being edited

**Recent Improvements:**
- ✅ Cancel button added (commit 620e2c0)
- ✅ Single-item edit enforcement (editing new item auto-saves previous)
- ✅ Input validation before save

**Technical Debt:**
- None - all major issues resolved

---

#### US-005: Delete Gift Item
**As a** logged-in user
**I want to** remove an item from my list
**So that** family members don't see items I no longer want

**Acceptance Criteria:**
- Click "Delete" button to remove item
- Item immediately disappears from view
- Success toast notification displays
- Claimed items can still be deleted by owner

**Current Implementation:**
- Component: `MyList.razor:157-161`
- Service: `googleSheetsListService.DeleteItem(itemId)`
- Soft delete: Sets Active = 0, doesn't remove from sheet
- No confirmation dialog

**Recent Improvements:**
- ✅ Confirmation dialog implemented (commit 1ea7d76)
- ✅ Prevents accidental deletions
- ✅ Bootstrap modal for consistency

**Technical Debt:**
- No "undo" functionality (low priority - confirmation is sufficient)
- Doesn't notify claimer if item was claimed (future: email notifications)

---

#### US-006: Manage Personal Notes
**As a** logged-in user
**I want to** add notes visible to all family members
**So that** I can share additional context about my list

**Acceptance Criteria:**
- View current notes in read-only mode
- Click "Edit Notes" to enter edit mode
- Textarea allows multi-line input
- Click "Save Notes" to commit
- Success toast notification displays
- Notes appear in List Review for other users

**Current Implementation:**
- Component: `MyList.razor:88-97`
- Service: `googleSheetsListService.UpdateNotes(userId, notes)`
- Toggle between view/edit with `editNotesFlag`
- Textarea 40 cols × 5 rows

**Recent Improvements:**
- ✅ Cancel button added (commit 620e2c0)
- ✅ Line breaks preserved with white-space:pre-wrap (commit 68265e7)

**Technical Considerations:**
- No character limit (intentional - allow lengthy notes)
- No rich text formatting (keep simple for family use)

---

### List Review (Browse Others' Lists)

#### US-007: Select Family Member's List
**As a** logged-in user
**I want to** choose which family member's list to view
**So that** I can see what they want

**Acceptance Criteria:**
- Display all family members except myself
- Shows summary stats (items listed, items claimed, time since update)
- Selecting a person loads their list immediately
- Color-coded claim status indicators
- Side-by-side layout (person selector | item display)

**Current Implementation (Major Redesign - commits d2f49ef, 71fb8ea):**
- Component: `ListReview.razor`
- Layout: Side-by-side panels (35% selector / 65% list display)
- Person Cards: Color-coded grid layout
  - Grey: No items on list
  - Blue: Has items, none claimed by current user
  - Green: Has items claimed by current user
- Metrics: 📝 items listed, 🎁 items claimed by current user, 🕐 time since update
- Responsive: Side-by-side on desktop/tablet, vertical stack on mobile
- Data: `allListsService.AllLists`
- Filter: Excludes current user

---

#### US-008: View Family Member's Items
**As a** logged-in user
**I want to** see items on a family member's list
**So that** I know what gifts they'd like

**Acceptance Criteria:**
- Card-based layout for visual appeal
- Each card shows: Product image, Item name, Notes, Link badge, Updated date
- Link badge clickable to open product page
- Claim status visible (Unclaimed / Claimed / Claimed by me)
- Empty list shows helpful guidance message
- Animations for claim/unclaim actions

**Current Implementation:**
- Component: `ListReview.razor`
- Layout: Bootstrap card grid (responsive, mobile-optimized)
- Product Images: Auto-displayed from metadata (commit 03dcb43)
- Card Footer: Vertical layout (timestamp above action button)
- Animations: Smooth claim/unclaim transitions (commit 7450565)
- Empty State: "👈 Select a person from the left to view and claim their gifts"

**Recent Improvements:**
- ✅ Mobile responsive layout fixed (commit d09b571)
- ✅ Product images displayed (commits 737ef6d, ee6c781, 87102af)
- ✅ Claim workflow animations (commit 7450565)
- ✅ Side-by-side layout optimization (commit 71fb8ea)

**Technical Considerations:**
- No sorting/filtering options (low priority for family use)
- No search functionality (lists typically small enough to browse)

---

#### US-009: Claim Gift Item
**As a** logged-in user
**I want to** mark an item as "claimed"
**So that** others know I'm purchasing it

**Acceptance Criteria:**
- "Claim!" button visible on unclaimed items
- Clicking claims item for current user
- Success toast: "You claimed [item] for [person]!"
- Button changes to "Unclaim"
- DateClaimed automatically recorded
- Updates immediately without page refresh

**Current Implementation:**
- Component: `ListReview.razor:89-94`
- Service: `googleSheetsListService.ClaimItem(itemId, userId)`
- Updates local state in `allListsService.AllLists` (line 92)
- Conditional rendering based on `item.Claimer`

**Recent Improvements:**
- ✅ Conflict detection implemented (commit e8f7740)
- ✅ Verifies item hasn't been claimed by another user before saving
- ✅ Error toast if claim conflict detected
- ✅ Smooth animations for claim actions (commit 7450565)

**Technical Debt:**
- No notification to list owner (future: email notifications)

---

#### US-010: Unclaim Gift Item
**As a** logged-in user
**I want to** remove my claim on an item
**So that** someone else can purchase it if my plans change

**Acceptance Criteria:**
- "Unclaim" button visible on items I've claimed
- Clicking removes claim
- Info toast: "Unclaimed [item]"
- Button changes to "Claim!"
- Claimer and DateClaimed cleared
- Cannot unclaim items claimed by others

**Current Implementation:**
- Component: `ListReview.razor:95-100`
- Service: `googleSheetsListService.UnclaimItem(itemId)`
- Updates local state (line 98)
- Different toast type (ShowInfo vs ShowSuccess)
- Conflict detection: Verifies still owned by current user (commit e8f7740)

---

#### US-011: View Family Member's Notes
**As a** logged-in user
**I want to** read the personal notes a family member shared
**So that** I have context for their list

**Acceptance Criteria:**
- Notes displayed below item cards
- Section labeled "Notes:"
- Plain text display
- Empty notes show empty paragraph

**Current Implementation:**
- Component: `ListReview.razor:78-80`
- Data: `allListsService.userList.Notes` (line 86)
- Read-only display

---

### My Gifts Tab (Claim Tracking)

#### US-012: View My Claimed Gifts
**As a** logged-in user
**I want to** see all items I've claimed across all family members
**So that** I have a shopping list

**Acceptance Criteria:**
- Table shows: Elf name, Item, Link, Date Claimed
- Includes items from all family members
- Only shows active items
- Links are clickable
- Sorted by date or family member

**Current Implementation:**
- Component: `MyGifts.razor:21-29`
- Query: `GetAllItmes().Where(i => i.Claimer == userId && i.Active == 1)`
- Table format (not cards)
- Read-only view

**Recent Improvements:**
- ✅ Typo fixed: `GetAllItmes` → `GetAllItems` (commit 287f0ec)
- ✅ Grouping by family member implemented (commit 09d4b8c)
- ✅ Unclaim functionality from this view (commit 09d4b8c)
- ✅ DateTime formatting with relative time display (commit 09d4b8c)
- ✅ Product images displayed (commit 03dcb43)
- ✅ Total count displayed per family member

**Technical Considerations:**
- No sorting controls (grouped by family member is sufficient)

---

## Technical Requirements

### TR-001: Google Sheets Integration
**Current State:**
- SpreadsheetId: `1vlS_UM0fCKAWPNxix8C2POwrcH3YTTdSpYQZWeDeG0k`
- Sheets: "Users", "Items"
- Authentication: OAuth2 with `app_client_secret.json`

**Requirements:**
- Read/write access to Google Sheets API v4
- Service account or OAuth credentials
- Error handling for API failures
- Rate limiting considerations

**Refactor Considerations:**
- Abstract data layer to support multiple backends
- Add caching to reduce API calls
- Implement retry logic with exponential backoff
- Add connection health checks

---

### TR-002: Data Models

#### UserModel
```csharp
public class UserModel {
    int Id
    string Family
    string Name
    string Password
    string Notes
}
```

**Refactor Needs:**
- Separate authentication concerns from user profile
- Add created/updated timestamps
- Add email field for future notifications
- Consider role-based access (admin, user)

---

#### ItemModel
```csharp
public class ItemModel {
    int ItemId
    string Name        // Owner's name
    string Item        // Item description
    string Link        // Product URL
    string Notes       // Item notes
    string DateUpdated // Last modified
    string Claimer     // Who claimed it
    string DateClaimed // When claimed
    int Active         // Soft delete flag (1=active, 0=deleted)

    // Product metadata (added commit 737ef6d)
    string ImageUrl           // Product image from OpenGraph/Google Image Search
    decimal? Price            // Nullable - may not always have price
    string MetadataFetchedDate // When metadata was last scraped
}
```

**Recent Improvements:**
- ✅ ImageUrl field added for product images
- ✅ Price field added (nullable decimal)
- ✅ MetadataFetchedDate tracks freshness

**Refactor Needs:**
- Dates should be DateTime, not string (deferred - see "Won't Do" in REFACTOR_BACKLOG.md)
- Add CreatedDate separate from DateUpdated (low priority)
- Add category/tags (future enhancement)
- Add priority level (future enhancement)
- Consider foreign key pattern for Name/Claimer (would require DB migration)

---

#### ListModel
```csharp
public class ListModel {
    List<ItemModel> List
    string Name
    int itemsListed
    int itemsClaimed
    DateTime lastUpdated
    string dropDownStr  // Computed display string
}
```

**Refactor Needs:**
- dropDownStr shouldn't be in model (presentation concern)
- Add computed properties properly
- Consider separating view model from domain model

---

### TR-003: Service Layer Architecture

**Current Services:**
- `googleSheetsListService` - Main CRUD operations, retry logic with exponential backoff
- `allListsService` - Global state cache (scoped), bulk data loading
- `userIdService` - Session state (scoped)
- `ProductMetadataService` - OpenGraph scraping, orchestrates image search (commit ee6c781)
- `GoogleImageSearchService` - Google Custom Search API integration (commit 87102af)
- `LocalStorageService` - Browser localStorage for "Remember Me" (commit 41e21fa)
- `googleSheetsSpiceService` - Secondary spreadsheet (legacy, appears unused)

**Refactor Strategy:**
1. **Repository Pattern**
   - `IUserRepository`, `IItemRepository`
   - `GoogleSheetsUserRepository`, `GoogleSheetsItemRepository`
   - Allow for future SQL/NoSQL implementations

2. **Service Layer**
   - `IListService` - Business logic
   - `IAuthenticationService` - Login/session
   - `INotificationService` - Toasts/alerts

3. **State Management**
   - Consider Fluxor or similar for Blazor state
   - Move away from shared service state
   - Implement proper caching strategy

---

### TR-004: Component Architecture

**Previous Pattern (Removed):**
- `Login.razor` - Base component with state
- `MyList.razor` - Inherited Login
- `ListReview.razor` - Inherited Login
- `MyGifts.razor` - Inherited Login

**Current Pattern (Refactored - commit 15c2b8c):**
- ✅ Component inheritance removed
- ✅ Shared state via service injection (`userIdService`, `allListsService`)
- ✅ Independent, reusable components
- ✅ Separation of concerns maintained

**Future Considerations:**
- Cascading parameters for user context (optional improvement)
- AuthenticationStateProvider pattern (optional for enterprise patterns)

---

### TR-005: Security Requirements

**Current Gaps:**
- Plain text passwords
- No HTTPS enforcement
- No CSRF protection
- No input sanitization
- No rate limiting on login
- Google Sheets credentials in codebase
- No audit logging

**Recent Improvements:**
- ✅ Credentials moved to configuration (commit ce3b34a)
- ✅ Input validation and sanitization implemented (commit 06004b6)

**Refactor Priorities (Updated):**
1. **DEFERRED**: Hash passwords - Intentional decision for family app (see REFACTOR_BACKLOG.md "Won't Do")
2. **MEDIUM**: Add HTTPS redirect
3. **MEDIUM**: CSRF tokens for forms
4. **MEDIUM**: Rate limiting on login
5. **LOW**: Two-factor authentication
6. **LOW**: Password reset flow
7. **LOW**: Azure Key Vault / AWS Secrets (currently environment variables sufficient)

---

### TR-006: UI/UX Requirements

**Current Implementation:**
- Bootstrap 5 (jQuery removed - commit ebb2f8a)
- Inter Bold font (modern sans-serif)
- Modern Christmas theme (Pine Green, Cranberry Red, Gold - commit accbed4)
- CSS variables system for theming
- Blazored.Toast for notifications
- Responsive tables and cards
- Custom component styling (components.css)

**Recent Improvements:**
- ✅ Upgraded Bootstrap 4 → 5, removed jQuery (commit ebb2f8a)
- ✅ Loading spinners for async operations (commit bd1f135)
- ✅ Mobile responsiveness improved (commits d09b571, 71fb8ea)
- ✅ Confirmation dialogs for destructive actions (commit 1ea7d76)
- ✅ Accessibility improvements: ARIA labels, keyboard navigation (commit bd1f135)
- ✅ Animations for claim/unclaim actions (commit 7450565)
- ✅ Modern Christmas theme with CSS variables (commit accbed4)
- ✅ Custom navigation with themed buttons (commit 68265e7)
- ✅ Enter key support for forms (commit 68265e7)

**Future Opportunities:**
- Dark mode toggle (deferred - current theme is well-established)
- Enhanced keyboard shortcuts (low priority)

---

### TR-007: Data Synchronization

**Current Challenges:**
- Multiple users editing simultaneously
- Google Sheets as single source of truth
- No real-time updates
- Optimistic UI updates can desync
- Manual state management in components

**Refactor Strategy:**
1. Implement SignalR for real-time updates
2. Add optimistic concurrency control
3. Implement proper cache invalidation
4. Add conflict resolution strategy
5. Consider moving to proper database

---

### TR-008: Error Handling

**Current State:**
- ✅ Comprehensive error handling implemented (commit 7a8f089)
- ✅ User-friendly toast notifications for errors
- ✅ Retry logic with exponential backoff (Google Sheets API)
- ✅ Graceful degradation (OpenGraph failures don't block image search)
- Console logging for debugging

**Recent Improvements:**
- ✅ Retry logic with exponential backoff (commit 7a8f089)
- ✅ User-friendly error messages via toasts (commit 7a8f089)
- ✅ Graceful degradation for metadata scraping (commit 67cdc43)
- ✅ Conflict detection for claims/unclaims (commit e8f7740)

**Future Requirements:**
- Global exception handler (low priority - current approach works well)
- Offline detection and queuing (low priority for online-only app)
- Error logging service like Application Insights or Sentry (optional)

---

### TR-009: Testing Requirements

**Current State:**
- No unit tests
- No integration tests
- No E2E tests

**Refactor Needs:**
1. **Unit Tests**
   - Service layer logic
   - Data transformations
   - Validation logic

2. **Integration Tests**
   - Google Sheets API interactions
   - Component integration
   - Service layer

3. **E2E Tests**
   - User workflows
   - Critical paths (login, add item, claim)

4. **Test Infrastructure**
   - xUnit or NUnit
   - bUnit for Blazor components
   - Moq for mocking
   - Playwright or Selenium for E2E

---

### TR-010: Performance Optimization

**Previous Issues (Resolved):**
- ~~Full data fetch on every component load~~ → ✅ Bulk loading at login (commit 7e947e8)
- ~~Repeated API calls~~ → ✅ 90% reduction via allListsService caching

**Current State:**
- ✅ Bulk data loading (commit 7e947e8)
- ✅ allListsService caching (scoped, one fetch per session)
- ✅ Optimistic UI updates (immediate feedback)
- ✅ Conflict detection prevents stale data issues

**Optimization Opportunities:**
1. Add pagination for large lists (low priority - family lists rarely exceed 50 items)
2. Lazy load images (low priority - product images are optimized)
3. Enable response compression (optional)
4. Add CDN for static assets (optional for family app)
5. Implement service worker for offline (low priority - online-only app)

---

## Feature Enhancements (Future)

### FE-001: Email Notifications
**User Story:**
As a user, I want to receive email when someone claims/unclaims my items, so I stay informed.

**Requirements:**
- SendGrid or similar integration
- Email preferences per user
- Digest vs real-time options
- Email templates

---

### FE-002: Budget Tracking
**User Story:**
As a user, I want to set a budget for gift-giving, so I don't overspend.

**Requirements:**
- Price field on items
- Budget setting per user
- Running total calculation
- Visual budget indicator

---

### FE-003: Multi-Family Support
**User Story:**
As a user, I want to participate in multiple family exchanges, so I can coordinate with different groups.

**Requirements:**
- Family/group entity
- User membership in multiple families
- Family switcher UI
- Separate lists per family

---

### FE-004: Gift Ideas / Suggestions
**User Story:**
As a user, I want to see gift suggestions based on interests, so I have inspiration.

**Requirements:**
- Interest/hobby tags
- Recommendation engine
- Integration with affiliate APIs (Amazon)
- AI-powered suggestions

---

### FE-005: Mobile App
**User Story:**
As a user, I want a native mobile app, so I can manage lists on the go.

**Requirements:**
- .NET MAUI or React Native
- Push notifications
- Camera for scanning barcodes
- Shared codebase with web

---

### FE-006: Gift Wrapping Tracker
**User Story:**
As a user, I want to track which gifts I've wrapped, so I stay organized.

**Requirements:**
- Wrapped status flag
- Filterable view
- Checklist mode

---

### FE-007: Anonymous Wishlist Sharing
**User Story:**
As a user, I want to share my wishlist with non-family via link, so friends can see what I want.

**Requirements:**
- Generate shareable link
- Public/private toggle per list
- Link expiration
- View-only access

---

## Migration Path

### Phase 1: Foundation (Weeks 1-2)
1. Add comprehensive logging
2. Implement proper error handling
3. Add unit test infrastructure
4. Document all existing APIs
5. Set up CI/CD pipeline

### Phase 2: Security (Weeks 3-4)
1. Hash passwords
2. Move credentials to secure storage
3. Add input validation
4. Implement HTTPS redirect
5. Add CSRF protection

### Phase 3: Architecture (Weeks 5-8)
1. Implement repository pattern
2. Refactor service layer
3. Remove component inheritance
4. Add proper routing
5. Implement state management

### Phase 4: Data Layer (Weeks 9-12)
1. Abstract Google Sheets integration
2. Add caching layer
3. Implement retry logic
4. Add real-time updates (SignalR)
5. Consider database migration

### Phase 5: UI/UX (Weeks 13-16)
1. Upgrade Bootstrap
2. Improve mobile responsiveness
3. Add loading states
4. Implement confirmation dialogs
5. Accessibility improvements

### Phase 6: Testing (Weeks 17-20)
1. Add unit tests (80% coverage)
2. Add integration tests
3. Add E2E tests for critical paths
4. Performance testing
5. Security testing

### Phase 7: Features (Weeks 21+)
1. Email notifications
2. Budget tracking
3. Enhanced search/filter
4. Multi-family support
5. Mobile app

---

## Technical Debt Items

### High Priority (Resolved)
1. ✅ **Component inheritance** - Removed (commit 15c2b8c)
2. ✅ **Error handling** - Comprehensive error handling with retry logic (commit 7a8f089)
3. ✅ **Typo in method name** - Fixed `GetAllItmes` → `GetAllItems` (commit 287f0ec)
4. **Password security** - Plain text storage (intentional - see REFACTOR_BACKLOG.md "Won't Do")
5. **No tests** - Zero test coverage (acknowledged - manual testing sufficient for family app)

### Medium Priority (Resolved)
1. ✅ **Confirmation dialogs** - Implemented (commit 1ea7d76)
2. ✅ **Input validation** - Comprehensive validation (commit 06004b6)
3. ✅ **Hard-coded credentials** - Moved to configuration (commit ce3b34a)
4. ✅ **Optimistic UI updates with conflict detection** - Implemented (commit e8f7740)

### Low Priority
1. **No pagination** - Could be slow with many items
2. **SpiceModel/SpiceService** - Appears unused
3. **dropDownStr in model** - Presentation in domain model
4. **String dates** - Should be DateTime
5. **Magic numbers** - Column widths, timeouts

---

## Code Quality Metrics

### Current State (Updated 2025-10-27)
- **Lines of Code**: ~3500 (estimated, includes CSS/JS)
- **Test Coverage**: 0%
- **Components**: 5 Razor components
- **Services**: 7 services (added ProductMetadataService, GoogleImageSearchService, LocalStorageService)
- **Models**: 4 models (ItemModel enhanced with metadata fields)
- **Dependencies**: 12+ NuGet packages
- **Technical Debt Ratio**: Low (estimated 5-10%, most major issues resolved)

### Target State
- **Test Coverage**: >80%
- **Code Duplication**: <3%
- **Maintainability Index**: >85
- **Cyclomatic Complexity**: <10 per method
- **Technical Debt Ratio**: <5%

---

## Notes
- Original copyright 2021 by AndrewLoeb
- MIT License
- Targeting .NET 9.0 (upgraded from .NET 5.0 - commit f6eee88)
- Active development since 2021
- Last major refactor session: 2025-10-27 (28 commits, comprehensive modernization)
- **Most Recent Improvements**: Side-by-side List Review layout, Google Custom Search integration, "Remember Me" functionality, Modern Christmas theme
