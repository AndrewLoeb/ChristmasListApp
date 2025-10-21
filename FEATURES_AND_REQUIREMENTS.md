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

**Technical Debt:**
- Passwords stored in plain text in Google Sheets
- No password hashing or encryption
- No session timeout mechanism
- No "remember me" functionality

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

**Current Implementation:**
- Component: `MyList.razor`
- Service: `googleSheetsListService.GetMyList(userId)`
- Filter: Active items only

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

**Technical Considerations:**
- No validation for duplicate items
- No character limit on fields
- No URL validation for links
- Manual form clearing (lines 142-144)

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

**Technical Debt:**
- No "Cancel" button to exit edit mode without saving
- No validation before save
- Clicking "Edit" on another item while editing doesn't save previous

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

**Technical Debt:**
- No confirmation dialog - accidental deletes possible
- No "undo" functionality
- Doesn't notify claimer if item was claimed

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

**Technical Considerations:**
- No character limit
- No rich text formatting
- No "Cancel" button to discard changes

---

### List Review (Browse Others' Lists)

#### US-007: Select Family Member's List
**As a** logged-in user
**I want to** choose which family member's list to view
**So that** I can see what they want

**Acceptance Criteria:**
- Dropdown shows all family members except myself
- Shows summary stats in dropdown (e.g., "Name (5 items, 2 claimed)")
- Selecting a name loads their list immediately
- Default state prompts "Select Elf..."

**Current Implementation:**
- Component: `ListReview.razor:24-34`
- Data: `allListsService.AllLists`
- Display: `List.dropDownStr` format
- Filter: Excludes current user (line 28)

---

#### US-008: View Family Member's Items
**As a** logged-in user
**I want to** see items on a family member's list
**So that** I know what gifts they'd like

**Acceptance Criteria:**
- Card-based layout for visual appeal
- Each card shows: Item name, Notes, Link badge, Updated date
- Link badge clickable to open product page
- Claim status visible (Unclaimed / Claimed / Claimed by me)
- Empty list shows no cards

**Current Implementation:**
- Component: `ListReview.razor:41-75`
- Layout: Bootstrap card-columns (responsive)
- Stretched link makes entire card clickable if link exists
- Color-coded badges for links and claim status

**Technical Considerations:**
- Card columns may not work well on mobile
- No image preview for products
- No sorting/filtering options
- No search functionality

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

**Technical Debt:**
- No claim limit (multiple people can claim if syncing issues)
- Optimistic UI update could be out of sync with sheets
- No notification to list owner

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

**Technical Considerations:**
- No grouping by family member
- No sorting controls
- No ability to unclaim from this view
- No total count displayed
- Typo in service method: `GetAllItmes` should be `GetAllItems`

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
    int Active         // Soft delete flag
}
```

**Refactor Needs:**
- Dates should be DateTime, not string
- Add CreatedDate separate from DateUpdated
- Add price/budget fields
- Add category/tags
- Add image URL
- Add priority level
- Consider foreign key pattern for Name/Claimer

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
- `googleSheetsListService` - Main CRUD operations
- `googleSheetsSpiceService` - Secondary spreadsheet (legacy?)
- `userIdService` - Session state (scoped)
- `allListsService` - Global state cache (scoped)

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

**Current Pattern:**
- `Login.razor` - Base component with state
- `MyList.razor` - Inherits Login
- `ListReview.razor` - Inherits Login
- `MyGifts.razor` - Inherits Login

**Problems:**
- Component inheritance is antipattern
- Tight coupling between components
- Shared state via inheritance
- Violates single responsibility

**Refactor Strategy:**
1. Remove inheritance
2. Use cascading parameters for user context
3. Create layout component for authenticated views
4. Separate concerns: Auth / Navigation / Content
5. Implement proper routing

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

**Refactor Priorities:**
1. **HIGH**: Hash passwords (bcrypt, Argon2)
2. **HIGH**: Move credentials to secure config (Azure Key Vault, AWS Secrets)
3. **HIGH**: Input validation and sanitization
4. **MEDIUM**: Add HTTPS redirect
5. **MEDIUM**: CSRF tokens for forms
6. **MEDIUM**: Rate limiting
7. **LOW**: Two-factor authentication
8. **LOW**: Password reset flow

---

### TR-006: UI/UX Requirements

**Current Implementation:**
- Bootstrap 4.x
- jQuery
- Font Awesome icons
- Blazored.Toast for notifications
- Responsive tables and cards

**Refactor Opportunities:**
1. **Upgrade Bootstrap** to v5 (remove jQuery dependency)
2. **Add loading states** for async operations
3. **Improve mobile responsiveness**
   - Card columns problematic on mobile
   - Table overflow issues
4. **Add confirmation dialogs** for destructive actions
5. **Implement dark mode**
6. **Add keyboard navigation**
7. **Improve accessibility** (ARIA labels, screen reader support)
8. **Add animations** for better UX

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
- Minimal error handling
- Console logging only
- No user-facing error messages for API failures
- No retry logic
- No offline support

**Requirements:**
1. Global exception handler
2. User-friendly error messages
3. Retry with exponential backoff
4. Offline detection and queuing
5. Error logging service (Application Insights, Sentry)
6. Graceful degradation

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

**Current Issues:**
- Full data fetch on every component load
- No pagination
- No lazy loading
- Repeated API calls
- No compression

**Optimization Opportunities:**
1. Implement caching strategy (Redis, MemoryCache)
2. Add pagination for large lists
3. Lazy load images/links
4. Reduce API calls with batching
5. Enable response compression
6. Add CDN for static assets
7. Implement service worker for offline

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

### High Priority
1. **Password security** - Plain text storage (Lines: Login.razor:87)
2. **Component inheritance** - Antipattern in Blazor
3. **Error handling** - No global handler or user feedback
4. **No tests** - Zero test coverage
5. **Typo in method name** - `GetAllItmes` in MyGifts.razor:34

### Medium Priority
1. **No confirmation dialogs** - Deletes are immediate
2. **Optimistic UI updates** - Can desync with backend
3. **No input validation** - URLs, character limits
4. **Manual state management** - Error-prone
5. **Hard-coded credentials** - SpreadsheetId in code

### Low Priority
1. **No pagination** - Could be slow with many items
2. **SpiceModel/SpiceService** - Appears unused
3. **dropDownStr in model** - Presentation in domain model
4. **String dates** - Should be DateTime
5. **Magic numbers** - Column widths, timeouts

---

## Code Quality Metrics

### Current State
- **Lines of Code**: ~2000 (estimated)
- **Test Coverage**: 0%
- **Components**: 5 Razor components
- **Services**: 4 services
- **Models**: 4 models
- **Dependencies**: 10+ NuGet packages
- **Technical Debt Ratio**: High (estimated 30%+)

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
- Targeting .NET 9.0
- Active development since 2021
- Last major update: 2024 (commit 6348687)
