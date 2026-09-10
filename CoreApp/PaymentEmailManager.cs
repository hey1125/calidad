using DTOs;
using System;
using System.Text;

namespace CoreApp
{
    public class PaymentEmailManager
    {
        private readonly EmailManager _emailManager;

        public PaymentEmailManager(string sendGridApiKey)
        {
            _emailManager = new EmailManager(sendGridApiKey);
        }

        /// <summary>
        /// Envía correos de confirmación a todas las partes involucradas en el pago
        /// HU 8.1: Enviar confirmación de pago por correo (SENDGRID)
        /// </summary>
        public void SendPaymentConfirmationEmails(Payment payment)
        {
            try
            {
                // Enviar correo al usuario final
                if (!string.IsNullOrEmpty(payment.PayerEmail))
                {
                    var userEmailContent = BuildUserEmailContent(payment);
                    _emailManager.SendEmail(
                        payment.PayerName ?? "Usuario",
                        payment.PayerEmail,
                        "Confirmación de Pago - BilleTico",
                        userEmailContent
                    );
                }

                // Enviar correo al comercio
                if (!string.IsNullOrEmpty(payment.CommerceEmail))
                {
                    var commerceEmailContent = BuildCommerceEmailContent(payment);
                    _emailManager.SendEmail(
                        payment.CommerceName ?? "Comercio",
                        payment.CommerceEmail,
                        "Notificación de Pago Recibido - BilleTico",
                        commerceEmailContent
                    );
                }

                // Enviar correo a la entidad financiera
                if (!string.IsNullOrEmpty(payment.BankEmail))
                {
                    var bankEmailContent = BuildBankEmailContent(payment);
                    _emailManager.SendEmail(
                        payment.BankName ?? "Entidad Financiera",
                        payment.BankEmail,
                        "Transacción Procesada - BilleTico",
                        bankEmailContent
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correos de confirmación: {ex.Message}");
                // En producción, considerar logging más robusto
            }
        }

        /// <summary>
        /// Construye el contenido del correo para el usuario final
        /// Criterio: Usuario: monto original y final, promoción, fecha/hora, comercio, cuenta usada
        /// </summary>
        private string BuildUserEmailContent(Payment payment)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .header { text-align: center; border-bottom: 2px solid #007bff; padding-bottom: 20px; margin-bottom: 30px; }");
            sb.AppendLine("        .title { color: #007bff; font-size: 24px; font-weight: bold; margin: 0; }");
            sb.AppendLine("        .subtitle { color: #666; font-size: 16px; margin: 5px 0 0 0; }");
            sb.AppendLine("        .content { line-height: 1.6; color: #333; }");
            sb.AppendLine("        .payment-details { background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }");
            sb.AppendLine("        .detail-row { display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }");
            sb.AppendLine("        .detail-label { font-weight: bold; color: #555; }");
            sb.AppendLine("        .detail-value { color: #333; }");
            sb.AppendLine("        .amount-highlight { font-size: 18px; font-weight: bold; color: #28a745; }");
            sb.AppendLine("        .promotion-box { background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 5px; padding: 15px; margin: 15px 0; }");
            sb.AppendLine("        .footer { margin-top: 30px; text-align: center; color: #666; font-size: 14px; border-top: 1px solid #eee; padding-top: 20px; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");

            // Header
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h1 class='title'>¡Pago Confirmado!</h1>");
            sb.AppendLine("            <p class='subtitle'>Su transacción ha sido procesada exitosamente</p>");
            sb.AppendLine("        </div>");

            // Saludo personalizado
            sb.AppendLine("        <div class='content'>");
            sb.AppendLine($"            <p>Estimado/a <strong>{payment.PayerName ?? "Usuario"}</strong>,</p>");
            sb.AppendLine("            <p>Le confirmamos que su pago ha sido procesado correctamente. A continuación los detalles de su transacción:</p>");

            // Detalles del pago
            sb.AppendLine("            <div class='payment-details'>");
            sb.AppendLine("                <h3 style='margin-top: 0; color: #007bff;'>Detalles de la Transacción</h3>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Código de Pago:</span>");
            sb.AppendLine($"                    <span class='detail-value'><strong>{payment.PaymentId}</strong></span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Fecha y Hora:</span>");
            sb.AppendLine($"                    <span class='detail-value'>{payment.PaymentDate:dd/MM/yyyy HH:mm}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Comercio:</span>");
            sb.AppendLine($"                    <span class='detail-value'>{payment.CommerceName ?? "N/A"}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Cuenta Utilizada:</span>");
            sb.AppendLine($"                    <span class='detail-value'>****{payment.IBAN?.Substring(Math.Max(0, payment.IBAN.Length - 4)) ?? "N/A"}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Monto Original:</span>");
            sb.AppendLine($"                    <span class='detail-value'>₡{payment.GrossAmount:N0}</span>");
            sb.AppendLine($"                </div>");

            // Mostrar promoción si existe
            if (payment.HasPromotion)
            {
                sb.AppendLine("            </div>");
                sb.AppendLine("            <div class='promotion-box'>");
                sb.AppendLine("                <h4 style='margin: 0 0 10px 0; color: #155724;'>🎉 ¡Promoción Aplicada!</h4>");
                sb.AppendLine($"                <p style='margin: 0;'><strong>Descuento:</strong> ₡{payment.AmountWDiscount:N0}</p>");
                if (!string.IsNullOrEmpty(payment.PromotionDescription))
                {
                    sb.AppendLine($"                <p style='margin: 5px 0 0 0; font-size: 14px;'>{payment.PromotionDescription}</p>");
                }
                sb.AppendLine("            </div>");
                sb.AppendLine("            <div class='payment-details'>");
            }

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label amount-highlight'>Monto Final:</span>");
            sb.AppendLine($"                    <span class='detail-value amount-highlight'>₡{payment.NetAmount:N0}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine("            </div>");

            // Mensaje de cierre
            sb.AppendLine("            <p>Gracias por utilizar BilleTico para sus transacciones. Su pago ha sido procesado de forma segura.</p>");
            sb.AppendLine("            <p>Si tiene alguna consulta sobre esta transacción, no dude en contactarnos.</p>");
            sb.AppendLine("        </div>");

            // Footer
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p><strong>BilleTico</strong><br>");
            sb.AppendLine("            Sistema de Pagos Digitales<br>");
            sb.AppendLine("            📧 notifiacionesbilletico@euucn.com</p>");
            sb.AppendLine("            <p><em>Este es un correo automático, por favor no responda a este mensaje.</em></p>");
            sb.AppendLine("        </div>");

            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// Construye el contenido del correo para el comercio
        /// Criterio: Comercio: monto original y final, promoción, fecha/hora, usuario, comisión cobrada
        /// </summary>
        private string BuildCommerceEmailContent(Payment payment)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .header { text-align: center; border-bottom: 2px solid #28a745; padding-bottom: 20px; margin-bottom: 30px; }");
            sb.AppendLine("        .title { color: #28a745; font-size: 24px; font-weight: bold; margin: 0; }");
            sb.AppendLine("        .subtitle { color: #666; font-size: 16px; margin: 5px 0 0 0; }");
            sb.AppendLine("        .content { line-height: 1.6; color: #333; }");
            sb.AppendLine("        .payment-details { background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }");
            sb.AppendLine("        .detail-row { display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }");
            sb.AppendLine("        .detail-label { font-weight: bold; color: #555; }");
            sb.AppendLine("        .detail-value { color: #333; }");
            sb.AppendLine("        .profit-highlight { font-size: 18px; font-weight: bold; color: #28a745; }");
            sb.AppendLine("        .commission-box { background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 15px 0; }");
            sb.AppendLine("        .footer { margin-top: 30px; text-align: center; color: #666; font-size: 14px; border-top: 1px solid #eee; padding-top: 20px; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");

            // Header
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h1 class='title'>💰 Pago Recibido</h1>");
            sb.AppendLine("            <p class='subtitle'>Nueva transacción procesada en su comercio</p>");
            sb.AppendLine("        </div>");

            // Saludo personalizado
            sb.AppendLine("        <div class='content'>");
            sb.AppendLine($"            <p>Estimado equipo de <strong>{payment.CommerceName ?? "Comercio"}</strong>,</p>");
            sb.AppendLine("            <p>Le notificamos que se ha procesado un nuevo pago en su comercio. A continuación los detalles:</p>");

            // Detalles del pago
            sb.AppendLine("            <div class='payment-details'>");
            sb.AppendLine("                <h3 style='margin-top: 0; color: #28a745;'>Información de la Transacción</h3>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Código de Pago:</span>");
            sb.AppendLine($"                    <span class='detail-value'><strong>{payment.PaymentId}</strong></span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Fecha y Hora:</span>");
            sb.AppendLine($"                    <span class='detail-value'>{payment.PaymentDate:dd/MM/yyyy HH:mm}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Cliente:</span>");
            sb.AppendLine($"                    <span class='detail-value'>{payment.PayerName ?? "N/A"}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Monto Original:</span>");
            sb.AppendLine($"                    <span class='detail-value'>₡{payment.GrossAmount:N0}</span>");
            sb.AppendLine($"                </div>");

            // Mostrar promoción si existe
            if (payment.HasPromotion)
            {
                sb.AppendLine($"                <div class='detail-row'>");
                sb.AppendLine($"                    <span class='detail-label'>Descuento Aplicado:</span>");
                sb.AppendLine($"                    <span class='detail-value'>₡{payment.AmountWDiscount:N0}</span>");
                sb.AppendLine($"                </div>");

                if (!string.IsNullOrEmpty(payment.PromotionDescription))
                {
                    sb.AppendLine($"                <div class='detail-row'>");
                    sb.AppendLine($"                    <span class='detail-label'>Promoción:</span>");
                    sb.AppendLine($"                    <span class='detail-value'>{payment.PromotionDescription}</span>");
                    sb.AppendLine($"                </div>");
                }
            }

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Monto Final:</span>");
            sb.AppendLine($"                    <span class='detail-value'>₡{payment.NetAmount:N0}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine("            </div>");

            // Información de comisión si existe
            if (payment.CoCommisionAmount.HasValue && payment.CoCommisionAmount > 0)
            {
                sb.AppendLine("            <div class='commission-box'>");
                sb.AppendLine("                <h4 style='margin: 0 0 10px 0; color: #856404;'>💼 Información Financiera</h4>");
                sb.AppendLine($"                <p style='margin: 0;'><strong>Comisión BilleTico:</strong> ₡{payment.CoCommisionAmount:N0}</p>");
                sb.AppendLine($"                <p style='margin: 5px 0 0 0;'><strong>Monto a Recibir:</strong> <span class='profit-highlight'>₡{payment.CommerceProfit:N0}</span></p>");
                sb.AppendLine("            </div>");
            }

            // Mensaje de cierre
            sb.AppendLine("            <p>El pago ha sido procesado exitosamente y será reflejado en su cuenta según los términos acordados.</p>");
            sb.AppendLine("            <p>Gracias por utilizar BilleTico como su plataforma de pagos digitales.</p>");
            sb.AppendLine("        </div>");

            // Footer
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p><strong>BilleTico</strong><br>");
            sb.AppendLine("            Sistema de Pagos Digitales<br>");
            sb.AppendLine("            📧 notifiacionesbilletico@euucn.com</p>");
            sb.AppendLine("            <p><em>Este es un correo automático, por favor no responda a este mensaje.</em></p>");
            sb.AppendLine("        </div>");

            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// Construye el contenido del correo para la entidad financiera
        /// Criterio: Entidad financiera: monto, fecha/hora, comercio, comisión cobrada
        /// </summary>
        private string BuildBankEmailContent(Payment payment)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .header { text-align: center; border-bottom: 2px solid #17a2b8; padding-bottom: 20px; margin-bottom: 30px; }");
            sb.AppendLine("        .title { color: #17a2b8; font-size: 24px; font-weight: bold; margin: 0; }");
            sb.AppendLine("        .subtitle { color: #666; font-size: 16px; margin: 5px 0 0 0; }");
            sb.AppendLine("        .content { line-height: 1.6; color: #333; }");
            sb.AppendLine("        .payment-details { background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }");
            sb.AppendLine("        .detail-row { display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }");
            sb.AppendLine("        .detail-label { font-weight: bold; color: #555; }");
            sb.AppendLine("        .detail-value { color: #333; }");
            sb.AppendLine("        .commission-highlight { font-size: 18px; font-weight: bold; color: #17a2b8; }");
            sb.AppendLine("        .footer { margin-top: 30px; text-align: center; color: #666; font-size: 14px; border-top: 1px solid #eee; padding-top: 20px; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");

            // Header
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h1 class='title'>🏦 Transacción Procesada</h1>");
            sb.AppendLine("            <p class='subtitle'>Notificación de transacción BilleTico</p>");
            sb.AppendLine("        </div>");

            // Saludo personalizado
            sb.AppendLine("        <div class='content'>");
            sb.AppendLine($"            <p>Estimado equipo de <strong>{payment.BankName ?? "Entidad Financiera"}</strong>,</p>");
            sb.AppendLine("            <p>Le informamos sobre una transacción procesada a través de la plataforma BilleTico:</p>");

            // Detalles del pago
            sb.AppendLine("            <div class='payment-details'>");
            sb.AppendLine("                <h3 style='margin-top: 0; color: #17a2b8;'>Detalles de la Transacción</h3>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Código de Transacción:</span>");
            sb.AppendLine($"                    <span class='detail-value'><strong>{payment.PaymentId}</strong></span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Fecha y Hora:</span>");
            sb.AppendLine($"                    <span class='detail-value'>{payment.PaymentDate:dd/MM/yyyy HH:mm}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Comercio:</span>");
            sb.AppendLine($"                    <span class='detail-value'>{payment.CommerceName ?? "N/A"}</span>");
            sb.AppendLine($"                </div>");

            sb.AppendLine($"                <div class='detail-row'>");
            sb.AppendLine($"                    <span class='detail-label'>Monto Procesado:</span>");
            sb.AppendLine($"                    <span class='detail-value'>₡{payment.NetAmount:N0}</span>");
            sb.AppendLine($"                </div>");

            // Mostrar comisión si existe
            if (payment.FeCommisionAmount.HasValue && payment.FeCommisionAmount > 0)
            {
                sb.AppendLine($"                <div class='detail-row'>");
                sb.AppendLine($"                    <span class='detail-label commission-highlight'>Comisión Generada:</span>");
                sb.AppendLine($"                    <span class='detail-value commission-highlight'>₡{payment.FeCommisionAmount:N0}</span>");
                sb.AppendLine($"                </div>");
            }

            sb.AppendLine("            </div>");

            // Mensaje de cierre
            sb.AppendLine("            <p>La transacción ha sido procesada exitosamente a través de su plataforma financiera.</p>");
            sb.AppendLine("            <p>Gracias por su colaboración con el ecosistema de pagos digitales BilleTico.</p>");
            sb.AppendLine("        </div>");

            // Footer
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p><strong>BilleTico</strong><br>");
            sb.AppendLine("            Sistema de Pagos Digitales<br>");
            sb.AppendLine("            📧 notifiacionesbilletico@euucn.com</p>");
            sb.AppendLine("            <p><em>Este es un correo automático, por favor no responda a este mensaje.</em></p>");
            sb.AppendLine("        </div>");

            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}