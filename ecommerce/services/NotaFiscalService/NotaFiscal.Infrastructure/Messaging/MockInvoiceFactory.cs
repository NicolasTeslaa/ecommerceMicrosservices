using System.Globalization;
using System.Text;
using ECommerce.Shared.Messaging;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Infrastructure.Messaging;

public class MockInvoiceFactory : IMockInvoiceFactory
{
    public Invoice Create(OrderConfirmedIntegrationEvent integrationEvent)
    {
        var issuedAtUtc = DateTime.UtcNow;
        var series = "1";
        var number = long.Parse(issuedAtUtc.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        var accessKey = BuildAccessKey(integrationEvent.OrderId, issuedAtUtc);
        var currency = string.IsNullOrWhiteSpace(integrationEvent.Currency) ? "brl" : integrationEvent.Currency;
        var xmlContent = BuildFakeXml(integrationEvent, number, series, accessKey, issuedAtUtc, currency);

        return new Invoice(
            integrationEvent.OrderId,
            integrationEvent.CustomerId,
            number,
            series,
            accessKey,
            xmlContent,
            integrationEvent.TotalAmount,
            currency,
            issuedAtUtc);
    }

    private static string BuildAccessKey(Guid orderId, DateTime issuedAtUtc)
    {
        var timestamp = issuedAtUtc.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var guidDigits = new string(orderId.ToString("N").Where(char.IsDigit).ToArray());
        var raw = $"{timestamp}{guidDigits}".PadRight(44, '0');
        return raw[..44];
    }

    private static string BuildFakeXml(
        OrderConfirmedIntegrationEvent integrationEvent,
        long number,
        string series,
        string accessKey,
        DateTime issuedAtUtc,
        string currency)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<MockNFe>");
        builder.AppendLine($"  <AccessKey>{accessKey}</AccessKey>");
        builder.AppendLine($"  <OrderId>{integrationEvent.OrderId}</OrderId>");
        builder.AppendLine($"  <CustomerId>{integrationEvent.CustomerId}</CustomerId>");
        builder.AppendLine($"  <Series>{series}</Series>");
        builder.AppendLine($"  <Number>{number}</Number>");
        builder.AppendLine($"  <IssuedAtUtc>{issuedAtUtc:O}</IssuedAtUtc>");
        builder.AppendLine("  <Status>Issued</Status>");
        builder.AppendLine($"  <Currency>{currency.ToLowerInvariant()}</Currency>");
        builder.AppendLine($"  <TotalAmount>{integrationEvent.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture)}</TotalAmount>");
        builder.AppendLine("  <Items>");

        foreach (var item in integrationEvent.Items)
        {
            builder.AppendLine("    <Item>");
            builder.AppendLine($"      <ProductId>{item.ProductId}</ProductId>");
            builder.AppendLine($"      <ProductName>{System.Security.SecurityElement.Escape(item.ProductName)}</ProductName>");
            builder.AppendLine($"      <Quantity>{item.Quantity}</Quantity>");
            builder.AppendLine($"      <UnitPrice>{item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture)}</UnitPrice>");
            builder.AppendLine($"      <TotalPrice>{item.TotalPrice.ToString("0.00", CultureInfo.InvariantCulture)}</TotalPrice>");
            builder.AppendLine("    </Item>");
        }

        builder.AppendLine("  </Items>");
        builder.AppendLine("</MockNFe>");
        return builder.ToString().Trim();
    }
}
