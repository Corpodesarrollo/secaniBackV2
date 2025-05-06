using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Request;
using Core.Services.StorageService;
using Infra.Repositories.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Repositorios
{
    public class ReportesSIVIGILARepo : IReportesSIVIGILARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<ReportesSIVIGILA> _repository;
        private readonly IStorageService _storageService;
        private readonly Lazy<INotificacionRepo> _notificacionRepo;

        public ReportesSIVIGILARepo(ApplicationDbContext context, IStorageService storageService, IServiceProvider serviceProvider)
        {
            _context = context;
            GenericRepository<ReportesSIVIGILA> repository = new(_context);
            _repository = repository;
            _storageService = storageService;
            _notificacionRepo = new Lazy<INotificacionRepo>(() => serviceProvider.GetRequiredService<INotificacionRepo>());
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

        public async Task<IEnumerable<ReportesSIVIGILADto>> GetAllPorEnviar(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repository.FindAllAsync(x => x.Estado == 0);
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
            entity.Estado = 0;
            var (success, response) = await _repository.AddAsync(entity);

            if (success)
            {
                if (data.EvidenciaDiagnostico != null)
                    await _storageService.UploadFileAsync(data.EvidenciaDiagnostico?.FileBytes, $"RS-EvidenciaDiagnostico-{entity.Id}-{data.NumeroIdentificacion}{data.EvidenciaDiagnostico.Extension}", true);

                if (data.EvidenciaParentesco != null)
                    await _storageService.UploadFileAsync(data.EvidenciaParentesco?.FileBytes, $"RS-EvidenciaParentesco-{entity.Id}-{data.NumeroIdentificacion}{data.EvidenciaParentesco.Extension}", true);

                await _notificacionRepo.Value.RevisarYEnviarNotificaciones();
            }

            return (success, response);
        }

        public async Task<UploadFileRequest?> EvidenciaDiagnostico(long id)
        {

            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return null;

            var dto = GenericMapper.Map<ReportesSIVIGILA, ReportesSIVIGILADto>(result);

            // Construcción del nombre del archivo
            string fileName = $"RS-EvidenciaDiagnostico-{dto.Id}-{dto.NumeroIdentificacion}{dto.EvidenciaDiagnostico?.Extension}";

            // Intentar descargar el archivo
            var evidenciaDiagnostico = await _storageService.DownloadFileAsync(fileName);

            // Validar si el archivo no existe o está vacío
            if (evidenciaDiagnostico == null || evidenciaDiagnostico.Length == 0)
                return null;

            return new UploadFileRequest
            {
                FileBytes = evidenciaDiagnostico,
                FileName = fileName
            };
        }

        public async Task<UploadFileRequest?> EvidenciaParentesco(long id)
        {

            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return null;

            var dto = GenericMapper.Map<ReportesSIVIGILA, ReportesSIVIGILADto>(result);

            string fileName = $"RS-EvidenciaParentesco-{dto.Id}-{dto.NumeroIdentificacion}{dto.EvidenciaParentesco?.Extension}";

            var evidenciaParentesco = await _storageService.DownloadFileAsync(fileName);

            // Validar si el archivo no existe o está vacío
            if (evidenciaParentesco == null || evidenciaParentesco.Length == 0)
                return null;

            return new()
            {
                FileBytes = evidenciaParentesco,
                FileName = $"RS-EvidenciaParentesco-{dto.Id}-{dto.NumeroIdentificacion}{dto.EvidenciaParentesco?.Extension}"
            };
        }

        public async Task<(bool, ReportesSIVIGILA)> UpdateAsync(ReportesSIVIGILADto data)
        {
            var entity = GenericMapper.Map<ReportesSIVIGILADto, ReportesSIVIGILA>(data);
            var (success, response) = await _repository.UpdateAsync(entity);
            return (success, response);
        }
    }
}
