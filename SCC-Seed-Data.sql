begin transaction

DECLARE @SchemeId UNIQUEIDENTIFIER = '713F8428-1D21-4072-9739-3FC31A6C90B6';
DECLARE @NewId UNIQUEIDENTIFIER = NEWID();

-- Throughput
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Throughput', 20, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'Up to & including 5000 a week', 5, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Up to & including 10000 a week', 10, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Over 10000 a week', 20, 1)

-- Operating hours
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Operating hours', 2, 2)

insert into [dbo].[Options]
values(NEWID(), @NewId, '6am - 6pm', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Out of hours', 20, 1)

-- Sign in prior to inspection
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Sign in prior to inspection', 6, 3)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'No', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Yes', 20, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Yes & phoning ahead', 30, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Yes, waiting & phoning ahead', 40, 1)

-- Trimming
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Trimming', 15, 4)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'No trimming', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'After scale point', 20, 1)

-- Skin Removal
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Skin Removal', 5, 5)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'Manual', 5, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Automated', 20, 1)

-- Carcass dressing
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Carcass dressing', 15, 6)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'Reference Spec', 25, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'UK Standard Spec', 20, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Both Specs', 30, 1)

-- Head Removal
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Head Removal', 10, 7)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'Manual', 5, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Automatic', 20, 1)

-- Feet Removal
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Feet Removal', 10, 8)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'Manual', 5, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'Automatic', 20, 1)

-- Scheme Year
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Scheme Year', 0, 9)

insert into [dbo].[Options]
values(NEWID(), @NewId, '2025', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, '2026', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, '2027', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, '2028', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, '2029', 0, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, '2030', 0, 1)

--
-- Historical scoring
--

-- Failed / Unsatisfactory 
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Failed / Unsatisfactory', 20, 10)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'Yes', 10, 1)
insert into [dbo].[Options]
values(NEWID(), @NewId, 'No', 0, 1)

-- How many fails / unsatisfactory in previous 12 Months
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'How many fails / unsatisfactory in previous 12 Months', 40, 11)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'No Fails / Unsatisfactory', 0, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '1', 2, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '2', 4, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '3', 6, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '4', 8, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '5', 10, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '6', 12, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '7', 14, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '8', 16, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '9', 18, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '10', 20, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'over 10', 30, 1)

-- How many near misses in previous 12 Months
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'How many near misses in previous 12 Months', 20, 12)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'No Near Misses', 0, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '1', 2, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '2', 4, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '3', 6, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '4', 8, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '5', 10, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '6', 12, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '7', 14, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '8', 16, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '9', 18, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '10', 20, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'over 10', 30, 1)

-- Number of Enforcement Notices Issued in Last 12 Months
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Number of Enforcement Notices Issued in Last 12 Months', 20, 13)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'No Enforcement Notices Issued', 0, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '1', 20, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '2', 25, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '3 or more', 30, 1)


-- Number of Penalty Notices Issued in Last 12 Months
SET @NewId = NEWID();
insert into [Criteria]
([CriteriaId], [SchemeId], [Name], [Weighting], [Order])
values (@NewId, @SchemeId, 'Number of Penalty Notices Issued in Last 12 Months', 20, 14)

insert into [dbo].[Options]
values(NEWID(), @NewId, 'No Penalty Notices Issued', 0, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '1', 20, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '2', 25, 1)

insert into [dbo].[Options]
values(NEWID(), @NewId, '3 or more', 30, 1)


commit
