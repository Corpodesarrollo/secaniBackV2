using Core.DTOs;
using Core.Modelos;
using Core.Modelos.Common;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infra.Repositorios.Procesos
{
    public class ProcesoActualizarEAPB(HttpClient httpClient, ApplicationDbContext db, DbContextOptions<ApplicationDbContext> dbContextOptions) : ProcesoAutomaticoBase
    {
        public override string Nombre => "ProcesoActualizarEAPB";
        private readonly string BASEURL = "https://web.sispro.gov.co/directoriogeneral/api/CodigoEAPByNit";

        protected override async Task EjecutarProcesoAsync(DateTime lastRun, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (lastRun == DateTime.MinValue)
                    lastRun = DateTime.Now.AddDays(-1); // Si no hay última ejecución, se establece un día atrás

                // Definir el tamaño del lote
                int batchSize = 10000;

                var data = await GetIPS();
                if (data.Length == 0)
                    return;

                //crear registros nuevos
                var newRecords = data.Where(x => x.Creation >= lastRun).ToArray();
                if (newRecords.Length == 0)
                    return;

                int totalNewRecords = newRecords.Length;
                int totalNewBatches = (int)Math.Ceiling((double)totalNewRecords / batchSize);

                for (int i = 0; i < totalNewBatches; i++)
                {
                    var batch = newRecords.Skip(i * batchSize).Take(batchSize).Select(dto => new TPEAPB
                    {
                        Codigo = dto.Codigo,
                        Nombre = dto.Nombre,
                        Descripcion = dto.Descripcion,
                        NIT = long.TryParse(dto.Extra_III, out long nit) ? nit : null,
                        DV = int.TryParse(dto.Extra_IV, out int dv) ? dv : null,
                        Creation = dto.Creation,
                        LastUpdate = dto.LastUpdate,
                        Tipo = ExtraerCode(dto.Extra_IX),
                    }).ToArray();
                    using var db = new ApplicationDbContext(dbContextOptions);
                    await db.TPEAPB.AddRangeAsync(batch);
                    await db.SaveChangesAsync();
                }

                //actualizar registros existentes
                var existingRecords = data.Where(x => x.LastUpdate >= lastRun).ToArray();
                if (existingRecords.Length == 0)
                    return;

                int totalExistingRecords = existingRecords.Length;
                int totalExistingBatches = (int)Math.Ceiling((double)totalExistingRecords / batchSize);

                for (int i = 0; i < totalExistingBatches; i++)
                {
                    var batch = existingRecords.Skip(i * batchSize).Take(batchSize).Select(dto => new TPEAPB
                    {
                        Codigo = dto.Codigo,
                        Nombre = dto.Nombre,
                        Descripcion = dto.Descripcion,
                        NIT = long.TryParse(dto.Extra_III, out long nit) ? nit : null,
                        DV = int.TryParse(dto.Extra_IV, out int dv) ? dv : null,
                        Creation = dto.Creation,
                        LastUpdate = dto.LastUpdate,
                        Tipo = ExtraerCode(dto.Extra_IX),
                    }).ToArray();
                    using var db = new ApplicationDbContext(dbContextOptions);
                    db.TPEAPB.UpdateRange(batch);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<ItemDto[]> GetIPS()
        {
            try
            {
                var response = await httpClient.GetAsync(BASEURL);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<RespuestaSisproDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data != null && data.Items != null)
                        return data.Items.ToArray();
                }

                return [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener los datos de la url: {ex.Message}");
                return [];
            }
        }

        private int? ExtraerCode(string? extra_IX)
        {
            if (string.IsNullOrEmpty(extra_IX))
                return null;

            if (extra_IX.StartsWith("1") || extra_IX.StartsWith("2"))
                return 1; // ET
            else if (extra_IX.StartsWith("10") || extra_IX.StartsWith("4") ||
                    extra_IX.StartsWith("5") || extra_IX.StartsWith("7") || extra_IX.StartsWith("8") || extra_IX.StartsWith("9"))
                return 2; // EAPB
            else
                return null; // No se puede determinar el tipo
        }

        public async Task<TPIPSDto[]?> GetMunicipio(string codeMunicipio)
        {
            try
            {
                var result = await db.TPIPS.Where(x => x.CodigoMunicipio == codeMunicipio).ToArrayAsync();
                if (result == null)
                    return null;

                var data = GenericMapper.Map<TPIPS[], TPIPSDto[]>(result);

                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TPIPSDto[]> GetAll()
        {
            try
            {
                var result = await db.TPIPS.ToArrayAsync();
                var data = GenericMapper.Map<TPIPS[], TPIPSDto[]>(result);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
