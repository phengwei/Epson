-- One-time data fix for the removal of the Demo Requisition Approval step.
-- Any request left waiting at ApprovalState = 120 (PendingDemoRequisitionApproval)
-- can no longer be actioned by a Director, since that approval gate has been removed
-- from the application. This moves those requests straight to Approved (50), matching
-- the outcome they would have received had a Director approved them.
--
-- Run this manually against the target database after deploying the code change.
UPDATE `Request`
SET `ApprovalState` = 50
WHERE `ApprovalState` = 120;
