# Contracts Business Logic - Quick Reference Guide

## Overview

The Contracts system enforces a critical business rule: **A player can have ONLY ONE active contract at any time.**

This is enforced through:
- ✅ Transactional service logic
- ✅ Database-level unique constraint
- ✅ Automatic deactivation mechanism

---

## API Endpoints

### Create Contract (Auto-Deactivates Previous)
```
POST /api/contracts
Authorization: Bearer {token}
Role: Admin, Manager

Body:
{
  "playerId": 1,
  "clubId": 2,
  "startDate": "2026-01-01",
  "endDate": "2028-12-31",
  "salary": 50000.00,
  "position": "Forward",
  "isActive": true  // ← Creates this as active, deactivates any previous active
}

Response:
{
  "success": true,
  "data": {
    "id": 1,
    "playerId": 1,
    "clubId": 2,
    "startDate": "2026-01-01",
    "endDate": "2028-12-31",
    "salary": 50000.00,
    "position": "Forward",
    "isActive": true,
    "createdAt": "2026-05-28T10:30:00Z",
    "updatedAt": "2026-05-28T10:30:00Z",
    "player": {...},
    "club": {...}
  },
  "message": "Contract created successfully"
}
```

### Get Active Contracts
```
GET /api/contracts/active?page=1&pageSize=10
Authorization: Bearer {token}

Response: List of only ACTIVE contracts (one per player max)
```

### Get All Contracts
```
GET /api/contracts?playerId={id}&page=1&pageSize=10
Authorization: Bearer {token}

Response: All contracts for filtering
```

### Update Contract
```
PUT /api/contracts/{id}
Authorization: Bearer {token}
Role: Admin, Manager

Body:
{
  "endDate": "2030-12-31",
  "salary": 55000.00,
  "position": "Forward",
  "isActive": true  // ← If activating, deactivates previous active
}

Note: If you set isActive=true on an inactive contract, 
      the previous active contract will be auto-deactivated
```

### Delete Contract
```
DELETE /api/contracts/{id}
Authorization: Bearer {token}
Role: Admin

Note: Deletes the contract. Does not affect other contracts.
```

---

## Behavior Examples

### Example 1: Auto-Deactivation on Create

**Step 1:** Create Contract A (Active)
```json
POST /api/contracts
{
  "playerId": 1,
  "clubId": 2,
  "isActive": true
}
→ Response: Contract A created with IsActive=true
```

**Step 2:** Create Contract B (Active) for same player
```json
POST /api/contracts
{
  "playerId": 1,  // ← Same player!
  "clubId": 3,
  "isActive": true
}
→ Response: Contract B created with IsActive=true
→ Contract A AUTOMATICALLY deactivated (IsActive=false)
```

**Step 3:** Query Active Contracts
```
GET /api/contracts/active
→ Returns: Only Contract B
→ Contract A is NOT returned (it's inactive)
```

---

### Example 2: Inactive Contract Doesn't Affect Active

**Step 1:** Contract A is active
```
Status: Active ✓
```

**Step 2:** Create Contract C as Inactive
```json
POST /api/contracts
{
  "playerId": 1,  // Same player
  "isActive": false  // ← Inactive!
}
→ Contract A remains ACTIVE ✓
→ Contract C created as INACTIVE
```

---

### Example 3: Update to Activate

**Step 1:** Contract A is active, Contract B is inactive
```
Contract A: Active ✓
Contract B: Inactive
```

**Step 2:** Update Contract B to Active
```json
PUT /api/contracts/B
{
  "isActive": true
}
→ Contract B becomes ACTIVE
→ Contract A automatically deactivated
```

---

## Testing

### Run All Tests
```bash
cd BackendAPI.Tests
dotnet test
```

### Test Coverage
- ✅ 22 comprehensive test cases
- ✅ All scenarios from requirements
- ✅ Edge cases
- ✅ Validation
- ✅ Pagination
- ✅ Multi-player scenarios

### Key Tests
```
✅ CreateContractAsync_WithIsActiveTrue_CreatesActiveContract
✅ CreateContractAsync_WithSecondActiveForSamePlayer_DeactivatesPrevious
✅ GetActiveContractsAsync_AfterAutoDeactivation_ReturnsOnlyNewContract
✅ MultipleActiveContracts_CanNeverExist_ForSamePlayer
✅ UpdateContractAsync_ActivatingInactiveContract_DeactivatesPrevious
```

---

## Critical Rules

### ✅ RULE 1: One Active Per Player
```
IF create/update with IsActive=true THEN
    deactivate all other active contracts for that player
END
```

### ✅ RULE 2: Inactive Never Deactivates
```
IF create/update with IsActive=false THEN
    do NOT touch other contracts
END
```

### ✅ RULE 3: Atomic Operation
```
IF error during create/update THEN
    rollback entire transaction (undo all changes)
END
```

### ✅ RULE 4: Database Protection
```
Database has unique index: UNIQUE(PlayerId WHERE Status=1)
This prevents ANY possibility of multiple active contracts
```

---

## Troubleshooting

### Issue: Getting multiple active contracts for same player
- **Cause:** Should not happen - database constraint prevents this
- **Solution:** Check database, verify unique index exists
- **Query:** 
  ```sql
  SELECT PlayerId, COUNT(*) as ActiveCount 
  FROM Contracts 
  WHERE Status = 1 
  GROUP BY PlayerId 
  HAVING COUNT(*) > 1
  ```
  Should return: No rows

### Issue: Old contract not deactivated
- **Cause:** Service logic error or transaction rollback
- **Solution:** Check service logs for exceptions
- **Verify:** 
  ```sql
  SELECT * FROM Contracts 
  WHERE PlayerId = {id} 
  ORDER BY UpdatedAt DESC
  ```

### Issue: Cannot create new active contract
- **Cause:** May be database constraint violation
- **Solution:** Check if player already has active contract
- **Query:**
  ```sql
  SELECT * FROM Contracts 
  WHERE PlayerId = {id} AND Status = 1
  ```

---

## Database

### Unique Constraint
```sql
CREATE UNIQUE INDEX IX_Contracts_PlayerId_Active 
ON Contracts(PlayerId) 
WHERE Status = 1
```

**Effect:**
- Only one row with Status=1 per PlayerId
- Database rejects violations
- Cannot be bypassed

### Contract Status Values
```
1 = Active       (currently under contract)
2 = Expired      (contract ended, auto-set)
3 = Terminated   (manually terminated)
4 = Suspended    (temporarily suspended)
5 = Pending      (not yet active)
```

---

## Developer Notes

### Code Quality
- ✅ Uses transactions for atomicity
- ✅ Proper exception handling
- ✅ Transaction rollback on error
- ✅ Navigation properties loaded
- ✅ DTOs properly mapped
- ✅ Follows project patterns

### Performance
- ✅ Indexed queries
- ✅ Pagination support
- ✅ Eager loading of relationships
- ✅ Efficient filtering

### Security
- ✅ Authorization required
- ✅ Role-based access (Admin, Manager)
- ✅ Input validation
- ✅ SQL injection prevention (EF Core)

---

## Deployment Checklist

Before deploying:

- [ ] Run all 22 tests - ensure they pass
- [ ] Verify database has unique index
- [ ] Check logs for any errors
- [ ] Test with real data
- [ ] Verify auto-deactivation in production
- [ ] Monitor for transaction timeouts
- [ ] Confirm active contracts are accurate
- [ ] Check no duplicate active contracts exist

---

## Contact

For issues or questions about the Contracts business logic:
1. Check this guide first
2. Review test cases for examples
3. Check CONTRACTS-VERIFICATION-COMPLETE.md for detailed info
4. Review ContractService.cs for implementation details

