namespace Ber2Bytes.Domain.Enums;

public enum UserRole { Admin = 1, Staff = 2 }

public enum PaymentMethod { Cash = 1, GCash = 2 }

public enum SaleStatus { Completed = 1, Voided = 2 }

public enum MovementType { StockIn = 1, Sale = 2, Adjustment = 3, Return = 4, Replacement = 5, Void = 6 }

public enum ReturnStatus { Pending = 1, Completed = 2, Rejected = 3 }

public enum ItemCondition { Resellable = 1, Defective = 2 }

public enum ReturnResolution { Refund = 1, Replacement = 2 }
