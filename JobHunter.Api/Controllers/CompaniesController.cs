using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Companies;
using JobHunter.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Api.Controllers
{
    [ApiController]
    [Route("api/v1/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Company>> Create([FromBody] ReqCreateCompanyDto dto, CancellationToken cancellationToken)
        {
            var company = await _companyService.CreateCompanyAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }

        [HttpGet]
        public async Task<ActionResult<List<Company>>> GetAll(CancellationToken cancellationToken)
        {
            var companies = await _companyService.GetCompaniesAsync(cancellationToken);
            return Ok(companies);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<Company>> Update([FromBody] ReqUpdateCompanyDto dto, CancellationToken cancellationToken)
        {
            var company = await _companyService.UpdateCompanyAsync(dto, cancellationToken);
            if (company == null) return NotFound();
            return Ok(company);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await _companyService.DeleteCompanyAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetById(long id, CancellationToken cancellationToken)
        {
            var company = await _companyService.GetCompanyByIdAsync(id, cancellationToken);
            if (company == null) return NotFound();
            return Ok(company);
        }
    }
}
