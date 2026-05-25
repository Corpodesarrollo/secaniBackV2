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
        private readonly string BASEURL = "https://web.sispro.gov.co/directoriogeneral/api/CodigoEAPByNit";
        //private readonly string BASEURL = "https://web.sispro.gov.co/directoriogeneral/api/IPSCodHabilitacion";

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
                    await db.TPEAPB.ExecuteDeleteAsync();
                }

                for (int i = 0; i < totalBatches; i++)
                {
                    //var batch = data.Skip(i * batchSize).Take(batchSize).Select(dto => new TPIPS
                    //{
                    //    Codigo = dto.Codigo,
                    //    Nombre = dto.Nombre,
                    //    Habilitado = dto.Habilitado,
                    //    CodigoMunicipio = dto.Extra_IV,
                    //    NombreMunicipio = dto.Extra_V,
                    //    Creation = dto.Creation,
                    //    LastUpdate = dto.LastUpdate
                    //}).ToArray();

                    var batch = data.Skip(i * batchSize).Take(batchSize).Select(dto => new TPEAPB
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

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar los datos a la base de datos: {ex.Message}");
                return false;
            }
        }

        private int? ExtraerCode(string? extra_IX)
        {
            if (string.IsNullOrEmpty(extra_IX))
                return null;

            if (extra_IX.StartsWith("1") || extra_IX.StartsWith("2"))
                return 1; // ET
            else if (extra_IX.StartsWith("10") || extra_IX.StartsWith("4") || extra_IX.StartsWith("5") || extra_IX.StartsWith("7") || extra_IX.StartsWith("8") || extra_IX.StartsWith("9"))
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

        public async Task<TPIPSDto?> GetIPSByCode(string code)
        {
            try
            {
                var result = await db.TPIPS.FirstOrDefaultAsync(x => x.Codigo == code);
                if (result == null)
                    return null;

                var data = GenericMapper.Map<TPIPS, TPIPSDto>(result);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TPIPSDto[]?> Search(string cadena)
        {
            try
            {
                var result = await db.TPIPS.Where(x => x.Codigo.Contains(cadena) || x.Nombre.Contains(cadena)).Take(50).ToArrayAsync();
                var data = GenericMapper.Map<TPIPS[], TPIPSDto[]>(result);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TPIPSDto?> GetById(long id)
        {
            try
            {
                var result = await db.TPIPS.FirstOrDefaultAsync(x => x.Id == id);
                if (result == null)
                    return null;
                var data = GenericMapper.Map<TPIPS, TPIPSDto>(result);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
