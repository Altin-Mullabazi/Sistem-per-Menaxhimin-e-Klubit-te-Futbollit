# P2-07 Implementation - Architecture Overview

## Project Structure

```
Frontend/src/
├── pages/
│   ├── Clubs.tsx                    ← NEW: Clubs management page
│   ├── Players.tsx                  ← UPDATED: Enhanced with pagination/filters
│   ├── Dashboard.tsx
│   ├── Login.tsx
│   └── SponsorsSeasons.tsx
├── components/
│   ├── ClubForm.tsx                 ← NEW: Club create/edit modal
│   ├── ClubList.tsx                 ← NEW: Club list with table/cards
│   ├── PlayerForm.tsx               ← UPDATED: Added clubId, jerseyNumber
│   ├── PlayerList.tsx               ← UPDATED: Added role-based access
│   ├── Navigation.tsx               ← UPDATED: Added Clubs link
│   ├── ProtectedRoute.tsx
│   └── Auth components
├── services/
│   ├── clubService.ts               ← NEW: Club API calls
│   ├── playerService.ts             ← UPDATED: Added pagination
│   ├── apiClient.ts
│   └── other services
├── styles/
│   ├── Management.css               ← NEW: Page layout styles
│   ├── List.css                     ← NEW: Table/card responsive styles
│   ├── Form.css                     ← UPDATED: Enhanced modal styles
│   ├── Navigation.css
│   └── other styles
├── types/
│   └── index.ts                     ← UPDATED: Club types added
├── context/
│   └── AuthContext.tsx              ← User auth, roles
└── App.tsx                          ← UPDATED: Added /clubs route
```

## Data Flow Architecture

### Clubs Management Flow
```
User navigates to /clubs
    ↓
Clubs.tsx loads
    ↓
useEffect: loads clubs page 1
    ↓
clubService.getClubs(1, 10, '', '')
    ↓
axios GET /api/clubs?page=1&pageSize=10
    ↓
Backend returns: { data: [...], totalPages, totalCount }
    ↓
setClubs() updates state
    ↓
ClubList renders table/cards
    ├── For each club: show name, city, founded year, etc.
    └── Show Edit/Delete buttons based on user role

User clicks Search
    ↓
debounceSearch (500ms)
    ↓
clubService.getClubs(1, 10, searchTerm, cityFilter)
    ↓
List updates with filtered results

User clicks Create
    ↓
ClubForm modal opens
    ↓
User fills form & clicks "Create Club"
    ↓
clubService.createClub(formData)
    ↓
axios POST /api/clubs { name, city, ... }
    ↓
Backend creates club, returns new club object
    ↓
Show success notification
    ↓
Reload clubs list from page 1
```

### Players Management Flow
```
User navigates to /players
    ↓
Players.tsx loads
    ↓
useEffect: 
  - Load players page 1
  - Load all clubs for filter dropdown
  - Set available positions
    ↓
playerService.getPlayers(1, 10, '', '', null)
    ↓
axios GET /api/players?page=1&pageSize=10
    ↓
Backend returns: { data: [...], totalPages, totalCount }
    ↓
setPlayers() updates state
    ↓
PlayerList renders table/cards
    ├── For each player: show name, position, age, jersey #, club
    └── Show Edit/Delete buttons based on user role

User applies filters
    ↓
Search by name → debounce 500ms
Filter by position → instant update
Filter by club → instant update
    ↓
playerService.getPlayers(1, 10, search, position, clubId)
    ↓
List updates with all filters applied
```

## Key Components

### 1. ClubForm.tsx
**Props:**
- club: Club | null (for edit mode)
- onClose: () => void
- onSubmit: () => void

**State:**
- formData: { name, city, logoUrl, foundedYear, president, budget }
- isLoading: boolean
- error: string | null

**Features:**
- Create: all fields except logoUrl required
- Edit: pre-fills data from club prop
- Validation: foundedYear 1800-current, budget is number
- Disable submit during request

### 2. ClubList.tsx
**Props:**
- clubs: Club[]
- isLoading: boolean
- onEdit: (club: Club) => void
- onDelete: (id: number) => void
- canEdit: boolean
- canDelete: boolean

**Display Modes:**
- Desktop (≥769px): HTML table with 7 columns
- Mobile (<769px): Card grid (1 card per row on mobile, responsive on tablet)

**Responsive Classes:**
- .list-desktop { display: none; } on mobile
- .list-mobile { display: block; } on mobile, hidden on desktop

### 3. Clubs.tsx Page
**State Management:**
- clubs: Club[] (current page data)
- currentPage, totalPages, pageSize=10
- searchTerm, cityFilter
- showForm, editingClub
- isLoading, error, success

**Key Functions:**
- loadClubs(page, search, city) - fetches from API
- handleSearchChange() - debounced 500ms
- handleCityFilter() - instant update
- handleCreateClick() - show form
- handleEditClick(club) - show form with club data
- handleFormSubmit() - reload list after create/edit
- handleDeleteClub(id) - delete with confirmation
- handlePageChange(page) - load different page

**Role-Based Permissions:**
```typescript
const canCreate = user?.role === 'Admin' || user?.role === 'Manager';
const canEdit = user?.role === 'Admin' || user?.role === 'Manager';
const canDelete = user?.role === 'Admin';
```

### 4. Players.tsx Page
**Similar to Clubs but with:**
- Additional position filter
- Additional club filter
- Multiple filters can be combined
- Filters trigger instantly (not debounced, only search is debounced)

**Filters Applied Together:**
```
loadPlayers(currentPage, searchTerm, positionFilter, clubId)
```

## Service Layer

### clubService.ts Methods

```typescript
// Get paginated list with filters
getClubs(page, pageSize, search?, city?): Promise<ClubListResponse>
  → GET /api/clubs?page=&pageSize=&search=&city=

// Get all clubs (no pagination)
getAllClubs(): Promise<Club[]>
  → GET /api/clubs?pageSize=1000

// Get single club with players
getClubById(id): Promise<ClubDetail>
  → GET /api/clubs/{id}

// Create new club
createClub(data): Promise<ApiResponse<Club>>
  → POST /api/clubs

// Update club
updateClub(id, data): Promise<ApiResponse<Club>>
  → PUT /api/clubs/{id}

// Delete club
deleteClub(id): Promise<ApiResponse<void>>
  → DELETE /api/clubs/{id}
```

### playerService.ts Methods

```typescript
// Get paginated list with filters
getPlayers(page, pageSize, search?, position?, clubId?): Promise<PlayerListResponse>
  → GET /api/players?page=&pageSize=&search=&position=&clubId=

// Get all players (no pagination)
getAllPlayers(): Promise<Player[]>
  → GET /api/players?pageSize=1000

// Get single player
getPlayerById(id): Promise<Player>
  → GET /api/players/{id}

// Create new player
createPlayer(data): Promise<ApiResponse<Player>>
  → POST /api/players

// Update player
updatePlayer(id, data): Promise<ApiResponse<Player>>
  → PUT /api/players/{id}

// Delete player
deletePlayer(id): Promise<ApiResponse<void>>
  → DELETE /api/players/{id}
```

## Type Definitions

```typescript
// Club Model
interface Club {
  id: number;
  name: string;
  city: string;
  logoUrl?: string;
  foundedYear: number;
  president?: string;
  budget?: number;
  playerCount: number;
  createdAt: string;
  updatedAt: string;
}

interface ClubListResponse {
  data: Club[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

interface CreateClubDto {
  name: string;
  city: string;
  logoUrl?: string;
  foundedYear: number;
  president?: string;
  budget?: number;
}

interface UpdateClubDto {
  name?: string;
  city?: string;
  // ... optional fields
}

// Player Model
interface Player {
  id: number;
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubId?: number;
  clubName?: string;
  jerseyNumber?: number;
  createdAt: string;
  updatedAt: string;
}

interface PlayerListResponse {
  data: Player[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

interface CreatePlayerDto {
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubId?: number;
  jerseyNumber?: number;
}

interface UpdatePlayerDto {
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubId?: number;
  jerseyNumber?: number;
}
```

## Styling Architecture

### Responsive Breakpoints
```css
Desktop (≥769px):
  - Show table view
  - Full toolbar in row
  - Normal button sizes

Tablet (600px - 768px):
  - Show card view
  - Toolbar wraps
  - Buttons adjusted

Mobile (<600px):
  - Show card view
  - Toolbar stacks vertically
  - Full-width buttons
  - Reduced padding
```

### CSS Classes

**Management Page Layout:**
- `.management-container` - max-width 1400px, centered
- `.management-header` - title and user info
- `.management-toolbar` - search, filters, action buttons
- `.pagination` - page navigation controls

**Alerts:**
- `.alert` - base alert styling
- `.alert-error` - red theme
- `.alert-success` - green theme

**Lists:**
- `.list-desktop` - table view (hidden on mobile)
- `.list-mobile` - card view (hidden on desktop)
- `.clubs-table` - main table
- `.club-card` - mobile card
- `.club-row` - hover effects

**Forms:**
- `.modal-overlay` - full-screen backdrop
- `.modal-content` - modal container
- `.form` - form element
- `.form-group` - input wrapper
- `.form-row` - two-column row
- `.form-actions` - submit/cancel buttons

## Authentication & Authorization

### Role-Based Access Control

```typescript
// In AuthContext
interface User {
  id: string;
  username: string;
  email: string;
  role?: string; // 'Admin', 'Manager', 'Coach', 'User'
}

// In component
const { user } = useAuth();

// Permissions
const canCreate = user?.role === 'Admin' || user?.role === 'Manager';
const canEdit = user?.role === 'Admin' || user?.role === 'Manager';
const canDelete = user?.role === 'Admin';

// Usage
{canDelete && (
  <button onClick={() => handleDelete(id)}>Delete</button>
)}
{!canDelete && <span className="view-only">View Only</span>}
```

## Error Handling

### Service Level
```typescript
try {
  const response = await apiClient.get('/clubs');
  return response.data.data;
} catch (error: any) {
  throw error.response?.data || { 
    success: false, 
    message: 'Failed to fetch clubs' 
  };
}
```

### Component Level
```typescript
try {
  await clubService.createClub(data);
  setSuccess('Club created successfully!');
} catch (err: any) {
  setError(err.message || 'Failed to create club');
}
```

### API Interceptor
```typescript
// In apiClient.ts
- 401 error → refresh token
- 403 error → unauthorized
- 500 error → server error
- Network error → connection issue
```

## Performance Optimizations

### Debounced Search
```typescript
const handleSearchChange = (e) => {
  setSearchTerm(e.target.value);
  
  if (searchTimeout) clearTimeout(searchTimeout);
  
  const timeout = setTimeout(() => {
    loadClubs(1, value, cityFilter);
  }, 500);
  
  setSearchTimeout(timeout);
};
```

### Responsive Images
```typescript
<img 
  src={club.logoUrl} 
  alt={club.name} 
  className="club-logo"
  // CSS: width: 32px, border-radius: 4px
/>
```

### Efficient Re-renders
- State properly split (UI state, data state, filter state)
- useCallback for event handlers
- Components only re-render when props change

## Browser Compatibility
- Modern browsers (Chrome, Firefox, Safari, Edge)
- CSS Grid for responsive layout
- Flexbox for component layout
- No IE11 support (uses modern ES6+)

## Accessibility
- Form labels linked to inputs (htmlFor)
- Semantic HTML (table, button, select)
- ARIA labels where needed
- Keyboard navigation (Tab, Enter)
- Focus states on interactive elements
- Color contrast meets WCAG standards

