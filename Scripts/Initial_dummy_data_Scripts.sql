-- 1. Insert into anbiyam
INSERT INTO anbiyam (anbiyam_name, anbiyam_code, anbiyam_zone, anbiyam_coordinator_name, anbiyam_ass_coordinator_name, coordinator_email, coordinator_phone)
VALUES 
('St. Peter', 'SP01', 1, 'John Peter', 'Mary Joseph', 'john.peter@example.com', '1234567890'),
('St. Paul', 'SP02', 2, 'Paul Raj', 'Anita Paul', 'paul.raj@example.com', '2345678901');

-- 2. Insert into family
INSERT INTO family (anbiyam_id, family_code, head_of_family, gender, family_permanant_address, family_temp_address, family_city, family_state, zip_code, phone, email, occupation, dob, qualification, blood_group, marriage_date, monthly_subscription, parish_member_since)
VALUES
(1, 'SP0101', 'Joseph Fernandez', 'Male', '123 Main St', '456 Temp St', 'Chennai', 'TN', '600001', '9876543210', 'joseph.f@example.com', 'Engineer', '1970-05-15', 'B.E.', 'A+', '1995-06-20', 200, 2000),
(2, 'SP0202', 'Anita Paul', 'Female', '789 Church Rd', '101 Guest Rd', 'Chennai', 'TN', '600002', '8765432109', 'anita.p@example.com', 'Teacher', '1975-08-22', 'M.A.', 'B+', '1998-11-10', 150, 2005);
-- 3. Insert into family_member
INSERT INTO family_member (family_id, first_name, last_name, relationship, gender, dob, member_status, occupation, qualification, blood_group, email, phone, marital_status, baptized_date)
VALUES
(1, 'Joseph', 'Fernandez', 'Head', 'Male', '1970-05-15', 'Active', 'Engineer', 'B.E.', 'A+', 'joseph.f@example.com', '9876543210', 'Married', '1980-06-20'),
(1, 'Mary', 'Fernandez', 'Spouse', 'Female', '1972-03-10', 'Active', 'Homemaker', 'B.A.', 'O+', 'mary.f@example.com', '9876543211', 'Married', '1982-07-15'),
(1, 'Peter', 'Fernandez', 'Son', 'Male', '2000-01-01', 'Active', 'Student', 'B.Sc.', 'A+', 'peter.f@example.com', '9876543212', 'Single', NULL),
(2, 'Anita', 'Paul', 'Head', 'Female', '1975-08-22', 'Active', 'Teacher', 'M.A.', 'B+', 'anita.p@example.com', '8765432109', 'Married', '1985-12-01'),
(2, 'Paul', 'Raj', 'Spouse', 'Male', '1973-12-05', 'Active', 'Accountant', 'B.Com.', 'B+', 'paul.raj@example.com', '8765432110', 'Married', '1986-01-10'),
(2, 'Rita', 'Raj', 'Daughter', 'Female', '2002-07-15', 'Active', 'Student', 'B.A.', 'O+', 'rita.raj@example.com', '8765432111', 'Single', NULL);

-- 4. Insert into family_subscription
INSERT INTO family_subscription (family_id, subscription_month, subscription_year, amount, payment_date, payment_status, remarks)
VALUES
(1, 1, 2024, 200, '2024-01-10', 'Paid', 'January subscription paid'),
(1, 2, 2024, 200, NULL, 'Pending', 'February subscription pending'),
(2, 1, 2024, 150, '2024-01-12', 'Paid', 'January subscription paid'),
(2, 2, 2024, 150, NULL, 'Pending', 'February subscription pending');

-- 5. Insert into cemetery_details
INSERT INTO cemetery_details (family_id, member_id, deceased_name, date_of_birth, date_of_death, burial_date, burial_place, grave_number, remarks)
VALUES
(1, NULL, 'Grandpa Fernandez', '1940-02-10', '2020-05-01', '2020-05-03', 'St. Mary Cemetery, Chennai', 'G101', 'Beloved grandfather'),
(2, 6, 'Rita Raj', '2002-07-15', '2023-12-20', '2023-12-22', 'St. Paul Cemetery, Chennai', 'G202', 'Daughter of Anita and Paul');
GO


-- Add more anbiyam (zones)
INSERT INTO anbiyam (anbiyam_name, anbiyam_code, anbiyam_zone, anbiyam_coordinator_name, anbiyam_ass_coordinator_name, coordinator_email, coordinator_phone)
VALUES 
('St. Thomas', 'ST03', 3, 'Thomas Xavier', 'Lucy Thomas', 'thomas.xavier@example.com', '3456789012'),
('St. Francis', 'SF04', 4, 'Francis Dsouza', 'Maria Francis', 'francis.dsouza@example.com', '4567890123'),
('St. Teresa', 'ST05', 5, 'Teresa Mary', 'John Teresa', 'teresa.mary@example.com', '5678901234');

-- Add more families (ensure anbiyam_id matches the new anbiyam rows above)
-- Suppose the new anbiyam_id values are 3, 4, 5 (check your DB for actual IDs)
INSERT INTO family (anbiyam_id, family_code, head_of_family, gender, family_permanant_address, family_temp_address, family_city, family_state, zip_code, phone, email, occupation, dob, qualification, blood_group, marriage_date, monthly_subscription, parish_member_since)
VALUES
(3, 'ST0303', 'Xavier Thomas', 'Male', '12 Beach Rd', '34 Guest St', 'Chennai', 'TN', '600003', '9123456780', 'xavier.t@example.com', 'Doctor', '1980-02-15', 'MBBS', 'B+', '2005-05-10', 250, 2010),
(4, 'SF0404', 'Maria Francis', 'Female', '56 Lake View', '78 Hill St', 'Chennai', 'TN', '600004', '9234567891', 'maria.f@example.com', 'Nurse', '1985-07-20', 'B.Sc.', 'O+', '2010-09-15', 180, 2012),
(5, 'ST0505', 'John Teresa', 'Male', '90 Park Lane', '12 River Rd', 'Chennai', 'TN', '600005', '9345678902', 'john.t@example.com', 'Engineer', '1978-11-30', 'B.E.', 'A-', '2000-03-25', 220, 2008);

-- Add more families to existing zones if needed
INSERT INTO family (anbiyam_id, family_code, head_of_family, gender, family_permanant_address, family_temp_address, family_city, family_state, zip_code, phone, email, occupation, dob, qualification, blood_group, marriage_date, monthly_subscription, parish_member_since)
VALUES
(1, 'SP0103', 'David Peter', 'Male', '101 Main St', '202 Temp St', 'Chennai', 'TN', '600006', '9456789012', 'david.p@example.com', 'Teacher', '1982-04-18', 'M.A.', 'AB+', '2007-08-12', 210, 2011),
(2, 'SP0203', 'Anil Paul', 'Male', '303 Church Rd', '404 Guest Rd', 'Chennai', 'TN', '600007', '9567890123', 'anil.p@example.com', 'Accountant', '1979-09-25', 'B.Com.', 'B-', '2002-12-05', 190, 2009);