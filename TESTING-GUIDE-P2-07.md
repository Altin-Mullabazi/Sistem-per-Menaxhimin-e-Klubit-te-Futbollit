# QUICK START: Testing Clubs & Players Pages

## Setup
```bash
# Terminal 1: Start Frontend
cd Frontend
npm run dev

# Terminal 2: Start Backend (if needed)
cd BackendAPI
dotnet run
```

Frontend will be at: `http://localhost:5173`
Backend API at: `http://localhost:5000/api`

## Quick Testing Flow

### Login First
1. Navigate to `http://localhost:5173/login`
2. Use test credentials:
   - Email: admin@test.com, Password: Admin@123 (Admin role)
   - OR Email: manager@test.com, Password: Manager@123 (Manager role)
   - OR Email: coach@test.com, Password: Coach@123 (Coach role)

### Test Clubs Page

**Navigate to Clubs:**
1. Click "Clubs" in navigation menu
2. You should see a list of clubs with:
   - Table on desktop / Cards on mobile
   - Search box for club name
   - City filter dropdown
   - "Add New Club" button (if Admin/Manager)

**Test Create:**
1. Click "+ Add New Club" button
2. Fill in form:
   - Club Name: "Test Club"
   - City: "New York"
   - Founded Year: 2020
   - President: "John Doe" (optional)
   - Budget: 1000000 (optional)
3. Click "Create Club"
4. See success notification

**Test Search:**
1. In search box, type part of a club name
2. Wait 500ms for search to trigger
3. List should filter automatically

**Test Filter:**
1. Click city filter dropdown
2. Select a city
3. List should filter by city

**Test Pagination:**
1. If more than 10 clubs, you'll see pagination buttons
2. Click "Next" to go to next page
3. Click "Previous" to go back

**Test Edit:**
1. Click "Edit" button on any club (if Admin/Manager)
2. Modal opens with pre-filled data
3. Change any field
4. Click "Update Club"
5. See success notification

**Test Delete:**
1. Click "Delete" button on any club (if Admin only)
2. Confirmation dialog appears
3. Click "OK" to confirm
4. Club is removed from list

**Test Mobile Responsive:**
1. Open DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Select mobile device
4. Verify cards display properly
5. Test that all buttons are accessible

### Test Players Page

**Navigate to Players:**
1. Click "Players" in navigation menu
2. You should see a list of players with:
   - Table on desktop / Cards on mobile
   - Search box for player name
   - Position filter dropdown
   - Club filter dropdown
   - "Add New Player" button (if Admin/Manager)

**Test Create:**
1. Click "+ Add New Player" button
2. Fill in form:
   - First Name: "John"
   - Last Name: "Doe"
   - Age: 25
   - Position: "Forward"
   - Jersey Number: 10 (optional)
   - Club: Select a club (optional)
3. Click "Create Player"
4. See success notification

**Test Search with Filters:**
1. Type player name in search
2. Select a position from dropdown
3. Select a club from dropdown
4. List filters by all criteria
5. Clear search box - search clears

**Test Combined Filters:**
1. Search for "Messi"
2. Filter by position "Forward"
3. Filter by club "Barcelona"
4. Should show only matches for all three criteria
5. Change one filter, list updates

**Test Pagination:**
1. If more than 10 players, pagination appears
2. Navigate between pages
3. Player count shows "Page X of Y (total)"

**Test Edit:**
1. Click "Edit" button (if Admin/Manager)
2. Form shows all fields pre-filled
3. Change jersey number or club
4. Click "Update Player"
5. See success notification

**Test Delete:**
1. Click "Delete" button (if Admin only)
2. Confirmation shown
3. Click "OK"
4. Player removed from list

### Test Role-Based Access

**As Admin:**
- Should see all CRUD buttons (Create, Edit, Delete)

**As Manager:**
- Should see Create and Edit buttons
- Should NOT see Delete button
- Should see "View Only" badge next to delete area

**As Coach/User:**
- Should NOT see Create button
- Should see "View Only" badge everywhere
- Can only view the lists

### Test Error Handling

**Invalid Create:**
1. Click Create
2. Leave required fields empty
3. Should see error message

**Invalid Update:**
1. Try to create player with age 150
2. Should see validation error

**Network Error:**
1. Stop backend server
2. Try to create something
3. Should see error message in alert

**Duplicate Submission:**
1. Open create form
2. Fill valid data
3. Click submit
4. Button should be disabled "Saving..."

### Test Console
1. Open DevTools Console (F12)
2. Should see NO errors
3. Should only see normal logs/warnings

## Checklist for PR

- [ ] Clubs page works
- [ ] Players page works
- [ ] Search works with debounce
- [ ] Filters work
- [ ] Pagination works
- [ ] Create/Edit/Delete work
- [ ] Role-based access works
- [ ] Mobile responsive works
- [ ] No console errors
- [ ] Notifications work
- [ ] Loading states work
- [ ] Empty states work

## Quick Debug Tips

**If Clubs not loading:**
1. Check backend is running: `dotnet run` in BackendAPI folder
2. Check API URL in `.env` or `apiClient.ts`
3. Check browser Network tab for API responses
4. Look for error in browser console

**If Search not working:**
1. Wait 500ms after typing (debounce)
2. Check that API returns pagination response
3. Check clubService getClubs() method

**If Mobile view broken:**
1. Check that `@media (max-width: 768px)` rules exist
2. Check List.css and Management.css
3. Clear browser cache (Ctrl+Shift+Del)

**If Buttons disabled:**
1. Check user role from localStorage
2. Verify auth context provides correct role
3. Check role checks in components
