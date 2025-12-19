# Decision Tables

## DT-1: Dynamic Pricing (PricePaid)

Rules:
- BasePrice by MembershipType:
  - Standard = 100
  - Premium  = 80
  - Student  = 70
- Time multiplier:
  - OffPeak = 1.00
  - Peak    = 1.20
- Occupancy multiplier:
  - Low  = 1.00
  - Mid  = 1.10
  - High = 1.30

Price = round(BasePrice * TimeMultiplier * OccupancyMultiplier, 2)

| Rule | Membership | TimeSlot | Occupancy | Expected Price |
|------|------------|----------|-----------|----------------|
| P1   | Standard   | OffPeak  | Low       | 100.00         |
| P2   | Standard   | OffPeak  | Mid       | 110.00         |
| P3   | Standard   | OffPeak  | High      | 130.00         |
| P4   | Standard   | Peak     | Low       | 120.00         |
| P5   | Standard   | Peak     | Mid       | 132.00         |
| P6   | Standard   | Peak     | High      | 156.00         |
| P7   | Premium    | OffPeak  | Low       | 80.00          |
| P8   | Premium    | OffPeak  | Mid       | 88.00          |
| P9   | Premium    | OffPeak  | High      | 104.00         |
| P10  | Premium    | Peak     | Low       | 96.00          |
| P11  | Premium    | Peak     | Mid       | 105.60         |
| P12  | Premium    | Peak     | High      | 124.80         |
| P13  | Student    | OffPeak  | Low       | 70.00          |
| P14  | Student    | OffPeak  | Mid       | 77.00          |
| P15  | Student    | OffPeak  | High      | 91.00          |
| P16  | Student    | Peak     | Low       | 84.00          |
| P17  | Student    | Peak     | Mid       | 92.40          |
| P18  | Student    | Peak     | High      | 109.20         |

## DT-2: Refund Policy (Cancel)

Rules (based on time before class start):
- GE24H  => 100% refund
- H2_24H => 50% refund
- LT2H   => 0% refund

| Rule | CancelWindow | TimeBeforeStart Example | Expected Refund Fraction |
|------|--------------|-------------------------|--------------------------|
| R1   | GE24H        | 25h                     | 1.00                     |
| R2   | H2_24H       | 10h                     | 0.50                     |
| R3   | LT2H         | 1h                      | 0.00                     |
