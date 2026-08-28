namespace SharedMessages;

public record PaymentProcessed(Guid OrderId, bool Success, string? PaymentIntentId);
