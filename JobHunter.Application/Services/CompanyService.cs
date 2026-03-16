using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Companies;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<Company> CreateCompanyAsync(ReqCreateCompanyDto dto, CancellationToken cancellationToken = default)
        {
            var company = new Company
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Logo = dto.Logo
            };
            await _companyRepository.AddAsync(company, cancellationToken);
            return company;
        }

        public async Task<Company?> UpdateCompanyAsync(ReqUpdateCompanyDto dto, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (company == null) return null;
            company.Name = dto.Name;
            company.Description = dto.Description;
            company.Address = dto.Address;
            company.Logo = dto.Logo;
            await _companyRepository.UpdateAsync(company, cancellationToken);
            return company;
        }

        public async Task DeleteCompanyAsync(long id, CancellationToken cancellationToken = default)
        {
            await _companyRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<Company?> GetCompanyByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _companyRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<List<Company>> GetCompaniesAsync(CancellationToken cancellationToken = default)
        {
            return await _companyRepository.GetAllAsync(cancellationToken);
        }
    }
}
