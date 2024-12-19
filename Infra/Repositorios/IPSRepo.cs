using Core.DTOs;
using Core.Interfaces;
using Core.Modelos;
using Core.Modelos.Common;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infra.Repositorios
{
    public class IpsRepo(HttpClient httpClient, ApplicationDbContext db, DbContextOptions<ApplicationDbContext> dbContextOptions) : IIpsRepo
    {
        private readonly string BASEURL = "https://web.sispro.gov.co/directoriogeneral/api/IPSCodHabilitacion";

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

                return Array.Empty<ItemDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener los datos de la url: {ex.Message}");
                return Array.Empty<ItemDto>();
            }
        }

        public async Task<bool> LoadData()
        {
            try
            {
                var data = await GetIPS();
                if (data.Length == 0)
                    return false;

                int batchSize = 10000;
                int totalRecords = data.Length;
                int totalBatches = (int)Math.Ceiling((double)totalRecords / batchSize);

                using (var db = new ApplicationDbContext(dbContextOptions))
                {
                    await db.TPIPS.ExecuteDeleteAsync();
                }

                for (int i = 0; i < totalBatches; i++)
                {
                    var batch = data.Skip(i * batchSize).Take(batchSize).Select(dto => new TPIPS
                    {
                        Codigo = dto.Codigo,
                        Nombre = dto.Nombre,
                        Habilitado = dto.Habilitado,
                        CodigoMunicipio = dto.Extra_IV,
                        NombreMunicipio = dto.Extra_V,
                        Creation = dto.Creation,
                        LastUpdate = dto.LastUpdate
                    }).ToArray();

                    using var db = new ApplicationDbContext(dbContextOptions);
                    await db.TPIPS.AddRangeAsync(batch);
                    await db.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar los datos a la base de datos: {ex.Message}");
                return false;
            }
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
