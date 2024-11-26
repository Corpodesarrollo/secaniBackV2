using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Services.StorageService;
using Infra.Repositories.Common;

namespace Infra.Repositorios
{
    public class ReportesSIVIGILARepo : IReportesSIVIGILARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<ReportesSIVIGILA> _repository;
        private readonly IStorageService _storageService;

        public ReportesSIVIGILARepo(ApplicationDbContext context, IStorageService storageService)
        {
            _context = context;
            GenericRepository<ReportesSIVIGILA> repository = new(_context);
            _repository = repository;
            _storageService = storageService;
        }

        public async Task<IEnumerable<ReportesSIVIGILADto>> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repository.GetAllAsync(cancellationToken);
                var dtos = GenericMapper.MapList<ReportesSIVIGILA, ReportesSIVIGILADto>(result);
                return dtos;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReportesSIVIGILADto?> GetById(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return null;

            var dto = GenericMapper.Map<ReportesSIVIGILA, ReportesSIVIGILADto>(result);
            return dto;
        }

        public async Task<(bool, ReportesSIVIGILA)> AddAsync(ReportesSIVIGILADto data)
        {
            var entity = GenericMapper.Map<ReportesSIVIGILADto, ReportesSIVIGILA>(data);
            var (success, response) = await _repository.AddAsync(entity);

            if (success)
            {
                if (data.EvidenciaDiagnostico != null)
                    await _storageService.UploadFileAsync(data.EvidenciaDiagnostico?.FileBytes, $"RS-EvidenciaDiagnostico-{entity.Id}-{data.NumeroIdentificacion}{data.EvidenciaDiagnostico.Extension}", true);

                if (data.EvidenciaParentesco != null)
                    await _storageService.UploadFileAsync(data.EvidenciaParentesco?.FileBytes, $"RS-EvidenciaParentesco-{entity.Id}-{data.NumeroIdentificacion}{data.EvidenciaParentesco.Extension}", true);
            }

            return (success, response);
        }

        public async Task<(bool, ReportesSIVIGILA)> UpdateAsync(ReportesSIVIGILADto data)
        {
            var entity = GenericMapper.Map<ReportesSIVIGILADto, ReportesSIVIGILA>(data);
            var (success, response) = await _repository.UpdateAsync(entity);
            return (success, response);
        }
    }
}
