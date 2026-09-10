using CoreApp;
using DataAccess.CRUD;
using DTOs;
using Newtonsoft.Json;

public class Program
{
    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== BILLETICO - BILLETERA VIRTUAL TESTER ===");
            Console.WriteLine("=== BILLETICO - BILLETERA VIRTUAL TESTER ===");
            Console.WriteLine();

            // REGISTRO
            Console.WriteLine("REGISTRO (HU 2.1):");
            Console.WriteLine("1. Crear entidad financiera");
            Console.WriteLine();

            Console.WriteLine("REGISTRO (HU 3.1):");
            Console.WriteLine("2. Crear comercio");
            Console.WriteLine();

            // ADMINISTRACIÓN DE ENTIDADES FINANCIERAS
            Console.WriteLine("ADMINISTRACIÓN (HU 2.2):");
            Console.WriteLine("3. Ver entidades pendientes");
            Console.WriteLine("4. Ver detalles de entidad");
            Console.WriteLine("5. Aprobar entidad");
            Console.WriteLine("6. Rechazar entidad");
            Console.WriteLine("7. Ver entidades activas");
            Console.WriteLine("8. Ver entidades rechazadas");
            Console.WriteLine("9. Verificar si entidad puede operar");
            Console.WriteLine();

            // ADMINISTRACIÓN DE COMERCIOS
            Console.WriteLine("ADMINISTRACIÓN (HU 3.2):");
            Console.WriteLine("10. Ver comercios pendientes");
            Console.WriteLine("11. Ver detalles de comercio");
            Console.WriteLine("12. Aprobar comercio");
            Console.WriteLine("13. Rechazar comercio");
            Console.WriteLine("14. Ver comercios activos");
            Console.WriteLine("15. Ver comercios rechazados");
            Console.WriteLine("16. Verificar si comercio puede operar");
            Console.WriteLine();

            // REGISTRO DE CUENTAS BANCARIAS
            Console.WriteLine("HU 4.1:");
            Console.WriteLine("17. Crear cuenta bancaria");

            //Vista de cuentas bancarias por usuario
            Console.WriteLine("HU 4.2:");
            Console.WriteLine("18. Ver cuentas bancarias por usuario");

            // SALIDA
            Console.WriteLine("0. Salir");
            Console.WriteLine();

            Console.Write("Seleccione una opción: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out int option))
            {
                Console.WriteLine("Opción inválida");
                Console.ReadKey();
                continue;
            }

            try
            {
                switch (option)
                {
                    case 1:
                        CreateFinancialEntity();
                        break;
                    case 2:
                        CreateCommerce();
                        break;
                    case 3:
                        ListPendingEntities();
                        break;
                    case 4:
                        ViewEntityDetails();
                        break;
                    case 5:
                        ApproveEntity();
                        break;
                    case 6:
                        RejectEntity();
                        break;
                    case 7:
                        ListActiveEntities();
                        break;
                    case 8:
                        ListRejectedEntities();
                        break;
                    case 9:
                        CheckEntityCanOperate();
                        break;
                    case 10:
                        ListPendingCommerces();
                        break;
                    case 11:
                        ViewCommerceDetails();
                        break;
                    case 12:
                        ApproveCommerce();
                        break;
                    case 13:
                        RejectCommerce();
                        break;
                    case 14:
                        ListActiveCommerces();
                        break;
                    case 15:
                        ListRejectedCommerces();
                        break;
                    case 16:  
                        CheckCommerceCanOperate();
                        break;
                    case 17:
                        CreateBankAccount();
                        break;
                    case 18:
                        ShowUserBankAccounts();
                        break;


                    case 0:
                        Console.WriteLine("¡Hasta luego!");
                        return;
                    default:
                        Console.WriteLine("Opción no válida");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\n Presione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }

    // =============================================
    // HU 2.1 - REGISTRO
    // =============================================
    private static void CreateFinancialEntity()
    {
        Console.WriteLine("\n=== CREAR ENTIDAD FINANCIERA (HU 2.1) ===");

        Console.Write("Cédula jurídica (10 dígitos): ");
        var legalId = Console.ReadLine();

        Console.Write("Código bancario (3 digitos): ");
        var bankCode = Console.ReadLine();

        Console.Write("Nombre de la entidad: ");
        var name = Console.ReadLine();

        Console.Write("Teléfono (8 dígitos): ");
        var phone = Console.ReadLine();

        Console.Write("Correo Electrónico: ");
        var email = Console.ReadLine();

        Console.Write("Latitud: ");
        var latitude = decimal.Parse(Console.ReadLine());

        Console.Write("Longitud: ");
        var longitude = decimal.Parse(Console.ReadLine());

        var financialEntity = new FinancialEntity()
        {
            LegalId = legalId,
            BankCode = bankCode,
            Name = name,
            Phone = phone,
            Email = email,
            Latitude = latitude,
            Longitude = longitude
        };

        var fem = new FinancialEntityManager();
        fem.Create(financialEntity);
        Console.WriteLine("Su solicitud ha sido recibida correctamente y está pendiente de aprobación.");
    }

    // =============================================
    // HU 3.1 - REGISTRO
    // =============================================
    private static void CreateCommerce()
    {
        Console.WriteLine("\n=== CREAR COMERCIO (HU 3.1) ===");

        Console.Write("Cédula jurídica (10 dígitos): ");
        var legalId = Console.ReadLine();

        Console.Write("Nombre del comercio: ");
        var name = Console.ReadLine();

        Console.Write("Teléfono (8 dígitos): ");
        var phone = Console.ReadLine();

        Console.Write("Correo Electrónico: ");
        var email = Console.ReadLine();

        Console.Write("IBAN: ");
        var IBAN = Console.ReadLine();

        Console.Write("Latitud: ");
        var latitude = decimal.Parse(Console.ReadLine());

        Console.Write("Longitud: ");
        var longitude = decimal.Parse(Console.ReadLine());


        var commerce = new Commerce()
        {
            LegalId = legalId,
            Name = name,
            Phone = phone,
            Email = email,
            Latitude = latitude,
            Longitude = longitude,
            IBAN = IBAN
        };

        var cem = new CommerceManager();
        cem.Create(commerce);
        Console.WriteLine("Su solicitud ha sido recibida correctamente y está pendiente de aprobación");
    }

    // =============================================
    // HU 2.2 - ADMINISTRACIÓN
    // =============================================

    // T15: Lista de entidades pendientes
    private static void ListPendingEntities()
    {
        Console.WriteLine("\n=== ENTIDADES PENDIENTES (T15) ===");

        var fem = new FinancialEntityManager();
        var pendingEntities = fem.GetPendingEntities();

        if (pendingEntities.Count == 0)
        {
            Console.WriteLine("📭 No hay entidades pendientes");
            return;
        }

        Console.WriteLine($"Se encontraron {pendingEntities.Count} entidades pendientes:\n");

        foreach (var entity in pendingEntities)
        {
            Console.WriteLine($"ID: {entity.Id} | Nombre: {entity.Name} | Código: {entity.BankCode} | Estado: {entity.Status}");
            Console.WriteLine($"   Cédula: {entity.LegalId} | Email: {entity.Email}");
            Console.WriteLine($"   Creado: {entity.Created:dd/MM/yyyy HH:mm}");
            Console.WriteLine();
        }
    }

    // T16: Ver detalles de solicitud
    private static void ViewEntityDetails()
    {
        Console.WriteLine("\n=== DETALLES DE ENTIDAD (T16) ===");

        Console.Write("Digite el ID de la entidad: ");
        var entityId = int.Parse(Console.ReadLine());

        var fem = new FinancialEntityManager();
        var entity = fem.GetEntityDetails(entityId);

        if (entity == null)
        {
            Console.WriteLine("Entidad no encontrada");
            return;
        }

        Console.WriteLine("\n DETALLES COMPLETOS:");
        Console.WriteLine($"ID: {entity.Id}");
        Console.WriteLine($"Cédula Jurídica: {entity.LegalId}");
        Console.WriteLine($"Código Bancario: {entity.BankCode}");
        Console.WriteLine($"Nombre: {entity.Name}");
        Console.WriteLine($"Teléfono: {entity.Phone}");
        Console.WriteLine($"Email: {entity.Email}");
        Console.WriteLine($"Coordenadas: {entity.Latitude}, {entity.Longitude}");
        Console.WriteLine($"Estado: {entity.Status}");
        Console.WriteLine($"Comisión: {entity.CommissionRate:F2}%");
        Console.WriteLine($"Creado: {entity.Created:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"Actualizado: {(entity.Updated == DateTime.MinValue ? "N/A" : entity.Updated.ToString("dd/MM/yyyy HH:mm"))}");
    }

    // T17: Aprobar entidad
    private static void ApproveEntity()
    {
        Console.WriteLine("\n=== APROBAR ENTIDAD (T17) ===");

        Console.Write("Digite el ID de la entidad a aprobar: ");
        var entityId = int.Parse(Console.ReadLine());

        Console.Write("Digite la comisión a asignar (ej: 2.50 para 2.50%): ");
        var commissionRate = decimal.Parse(Console.ReadLine());

        var fem = new FinancialEntityManager();
        fem.ApproveEntity(entityId, commissionRate);

        Console.WriteLine("Entidad aprobada exitosamente");
        Console.WriteLine($"Estado actualizado a 'Activa' con comisión del {commissionRate:F2}%");
        Console.WriteLine("La entidad ya puede operar en la plataforma");
    }

    // T17: Rechazar entidad
    private static void RejectEntity()
    {
        Console.WriteLine("\n=== RECHAZAR ENTIDAD (T17) ===");

        Console.Write("Digite el ID de la entidad a rechazar: ");
        var entityId = int.Parse(Console.ReadLine());

        var fem = new FinancialEntityManager();
        fem.RejectEntity(entityId);

        Console.WriteLine("Entidad rechazada exitosamente");
        Console.WriteLine("Estado actualizado a 'Rechazada'");
        Console.WriteLine("Se debería enviar notificación a la entidad (T18)");
    }

    // Ver entidades activas
    private static void ListActiveEntities()
    {
        Console.WriteLine("\n=== ENTIDADES ACTIVAS ===");

        var fem = new FinancialEntityManager();
        var activeEntities = fem.GetActiveEntities();

        if (activeEntities.Count == 0)
        {
            Console.WriteLine("No hay entidades activas");
            return;
        }

        Console.WriteLine($"Se encontraron {activeEntities.Count} entidades activas:\n");

        foreach (var entity in activeEntities)
        {
            Console.WriteLine($"ID: {entity.Id} | Nombre: {entity.Name} | Código: {entity.BankCode}");
            Console.WriteLine($"   Comisión: {entity.CommissionRate:F2}% | Estado: {entity.Status}");
            Console.WriteLine($"   Aprobado: {entity.Updated:dd/MM/yyyy HH:mm}");
            Console.WriteLine();
        }
    }

    // Ver entidades rechazadas
    private static void ListRejectedEntities()
    {
        Console.WriteLine("\n=== ENTIDADES RECHAZADAS ===");

        var fem = new FinancialEntityManager();
        var rejectedEntities = fem.GetRejectedEntities();

        if (rejectedEntities.Count == 0)
        {
            Console.WriteLine("No hay entidades rechazadas");
            return;
        }

        Console.WriteLine($"❌ Se encontraron {rejectedEntities.Count} entidades rechazadas:\n");

        foreach (var entity in rejectedEntities)
        {
            Console.WriteLine($"ID: {entity.Id} | Nombre: {entity.Name} | Código: {entity.BankCode}");
            Console.WriteLine($"   Estado: {entity.Status} | Rechazado: {entity.Updated:dd/MM/yyyy HH:mm}");
            Console.WriteLine();
        }
    }

    // T19: Verificar si entidad puede operar
    private static void CheckEntityCanOperate()
    {
        Console.WriteLine("\n=== VERIFICAR OPERATIVIDAD (T19) ===");

        Console.Write("Digite el ID de la entidad: ");
        var entityId = int.Parse(Console.ReadLine());

        var fem = new FinancialEntityManager();
        var canOperate = fem.CanEntityOperate(entityId);

        if (canOperate)
        {
            Console.WriteLine("La entidad PUEDE operar (estado: Activa)");
            Console.WriteLine("Puede acceder a promociones, cobros y otras funciones");
        }
        else
        {
            Console.WriteLine("La entidad NO PUEDE operar");
            Console.WriteLine("Solo entidades con estado 'Activa' pueden operar");
        }
    }

    // =============================================
    // HU 3.2 - ADMINISTRACIÓN
    // =============================================

    // T24: Lista de comercios pendientes de revisión
    private static void ListPendingCommerces()
    {
        Console.WriteLine("\n=== COMERCIOS PENDIENTES (T24) ===");

        var cem = new CommerceManager();
        var pendingCommerces = cem.GetPendingCommerce();

        if (pendingCommerces.Count == 0)
        {
            Console.WriteLine("No hay comercios pendientes");
            return;
        }

        Console.WriteLine($" Se encontraron {pendingCommerces.Count} comercios pendientes:\n");

        foreach (var commerce in pendingCommerces)
        {
            Console.WriteLine($"ID: {commerce.Id} | Nombre: {commerce.Name} | Email: {commerce.Email} | Estado: {commerce.Status}");
            Console.WriteLine($"   Cédula: {commerce.LegalId} | IBAN: {commerce.IBAN}");
            Console.WriteLine($"   Creado: {commerce.Created:dd/MM/yyyy HH:mm}");
            Console.WriteLine();
        }
    }

    // T25: Ver detalles de solicitud
    private static void ViewCommerceDetails()
    {
        Console.WriteLine("\n=== DETALLES DE COMERCIO (T25) ===");

        Console.Write("Digite el ID del comercio: ");
        var commerceId = int.Parse(Console.ReadLine());

        var cem = new CommerceManager();
        var commerce = cem.GetEntityDetails(commerceId);

        if (commerce == null)
        {
            Console.WriteLine("Comercio no encontrado");
            return;
        }

        Console.WriteLine("\n DETALLES COMPLETOS:");
        Console.WriteLine($"ID: {commerce.Id}");
        Console.WriteLine($"Cédula Jurídica: {commerce.LegalId}");
        Console.WriteLine($"Nombre: {commerce.Name}");
        Console.WriteLine($"Teléfono: {commerce.Phone}");
        Console.WriteLine($"Email: {commerce.Email}");
        Console.WriteLine($"IBAN: {commerce.IBAN}");
        Console.WriteLine($"Coordenadas: {commerce.Latitude}, {commerce.Longitude}");
        Console.WriteLine($"Estado: {commerce.Status}");
        Console.WriteLine($"Comisión: {commerce.CommissionRate:F2}%");
        Console.WriteLine($"Creado: {commerce.Created:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"Actualizado: {(commerce.Updated == DateTime.MinValue ? "N/A" : commerce.Updated.ToString("dd/MM/yyyy HH:mm"))}");
    }

    // T26: Aprobar comercio
    private static void ApproveCommerce()
    {
        Console.WriteLine("\n=== APROBAR COMERCIO (T26) ===");

        Console.Write("Digite el ID del comercio a aprobar: ");
        var commerceId = int.Parse(Console.ReadLine());

        Console.Write("Digite la comisión a asignar (ej: 2.50 para 2.50%): ");
        var commissionRate = decimal.Parse(Console.ReadLine());

        var cem = new CommerceManager();
        cem.ApproveCommerce(commerceId, commissionRate);

        Console.WriteLine("Comercio aprobado exitosamente");
        Console.WriteLine($"Estado actualizado a 'Activo' con comisión del {commissionRate:F2}%");
        Console.WriteLine("El comercio ya puede operar en BilleTico");
        Console.WriteLine("Se debería enviar notificación al comercio (T27)");
    }

    // T26: Rechazar comercio
    private static void RejectCommerce()
    {
        Console.WriteLine("\n=== RECHAZAR COMERCIO (T26) ===");

        Console.Write("Digite el ID del comercio a rechazar: ");
        var commerceId = int.Parse(Console.ReadLine());

        var cem = new CommerceManager();
        cem.RejectCommerce(commerceId);

        Console.WriteLine("Comercio rechazado exitosamente");
        Console.WriteLine("Estado actualizado a 'Rechazado'");
        Console.WriteLine("Se debería enviar notificación al comercio (T27)");
    }

    // Ver comercios activos
    private static void ListActiveCommerces()
    {
        Console.WriteLine("\n=== COMERCIOS ACTIVOS ===");

        var cem = new CommerceManager();
        var activeCommerces = cem.GetActiveCommerces();

        if (activeCommerces.Count == 0)
        {
            Console.WriteLine("No hay comercios activos");
            return;
        }

        Console.WriteLine($"Se encontraron {activeCommerces.Count} comercios activos:\n");

        foreach (var commerce in activeCommerces)
        {
            Console.WriteLine($"ID: {commerce.Id} | Nombre: {commerce.Name} | IBAN: {commerce.IBAN}");
            Console.WriteLine($"   Comisión: {commerce.CommissionRate:F2}% | Estado: {commerce.Status}");
            Console.WriteLine($"   Aprobado: {commerce.Updated:dd/MM/yyyy HH:mm}");
            Console.WriteLine();
        }
    }

    // Ver comercios rechazados
    private static void ListRejectedCommerces()
    {
        Console.WriteLine("\n=== COMERCIOS RECHAZADOS ===");

        var cem = new CommerceManager();
        var rejectedCommerces = cem.GetRejectedCommerces();

        if (rejectedCommerces.Count == 0)
        {
            Console.WriteLine("No hay comercios rechazados");
            return;
        }

        Console.WriteLine($"Se encontraron {rejectedCommerces.Count} comercios rechazados:\n");

        foreach (var commerce in rejectedCommerces)
        {
            Console.WriteLine($"ID: {commerce.Id} | Nombre: {commerce.Name} | IBAN: {commerce.IBAN}");
            Console.WriteLine($"   Estado: {commerce.Status} | Rechazado: {commerce.Updated:dd/MM/yyyy HH:mm}");
            Console.WriteLine();
        }
    }

    // T28: Verificar si un comercio puede operar
    private static void CheckCommerceCanOperate()
    {
        Console.WriteLine("\n=== VERIFICAR OPERATIVIDAD (T28) ===");

        Console.Write("Digite el ID del comercio: ");
        var commerceId = int.Parse(Console.ReadLine());

        var cem = new CommerceManager();
        var canOperate = cem.CanCommerceOperate(commerceId);

        if (canOperate)
        {
            Console.WriteLine("El comercio PUEDE operar (estado: Activa)");
            Console.WriteLine("Puede acceder a promociones, cobros y otras funciones");
        }
        else
        {
            Console.WriteLine("El comercio NO PUEDE operar");
            Console.WriteLine("Solo comercios con con estado 'Activo' pueden operar");
        }
    }
    // =============================================
    // HU 4.1 - CREAR CUENTA BANCARIA
    // =============================================
    
    private static void CreateBankAccount()
    {
        Console.WriteLine("\n=== CREAR CUENTA BANCARIA (HU 4.1) ===");

        Console.Write("ID del usuario (UserId): ");
        var userId = int.Parse(Console.ReadLine());

        Console.Write("ID del banco (BankId): ");
        var bankId = int.Parse(Console.ReadLine());

        Console.Write("IBAN: ");
        var iban = Console.ReadLine();

        var account = new BankAccount()
        {
            UserId = userId,
            BankId = bankId,
            IBAN = iban
        };

        var bam = new BankAccountManager();
        bam.Create(account);

        Console.WriteLine(" Cuenta bancaria creada correctamente.");
    }
    // =============================================
    // HU 4.2 - VER CUENTAS BANCARIAS POR USUARIO
    // =============================================
    private static void ShowUserBankAccounts()
    {
        Console.WriteLine("\n=== CUENTAS BANCARIAS DEL USUARIO (HU 4.2) ===");

        Console.Write("Digite el ID del usuario: ");
        var userId = int.Parse(Console.ReadLine());

        var bam = new BankAccountManager();
        var accounts = bam.GetAccountsByUser(userId);

        if (accounts.Count == 0)
        {
            Console.WriteLine(" El usuario no tiene cuentas registradas.");
            return;
        }

        Console.WriteLine($"\n Se encontraron {accounts.Count} cuentas:\n");

        foreach (var acc in accounts)
        {
            // Mostrar solo los últimos 4 dígitos del IBAN
            var last4 = acc.IBAN.Length >= 4 ? acc.IBAN[^4..] : acc.IBAN;

            // Obtener nombre del banco si lo necesitas (opcional)
            var banco = acc.BankId; // puedes reemplazar por nombre si tienes relación

            Console.WriteLine($"Cuenta en banco ID {banco} - IBAN **** **** **** {last4}");
        }
    }


}