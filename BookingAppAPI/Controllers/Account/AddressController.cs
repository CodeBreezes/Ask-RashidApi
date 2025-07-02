using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB; 
using BookingAppAPI.DB.Models.Address;

namespace Bpst.API.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // GET: api/Cities 
        [HttpGet("GetAllCities")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            return await _context.Cities.ToListAsync();
        }

        // GET: api/Cities/5
        [HttpGet("GetCitiesByStateId/{id}")]
        public async Task<ActionResult<IEnumerable<City>>> GetCitiesByStateId(int id)
        {
            var cities = await _context.Cities.Where(c => c.StateId == id).ToListAsync();
            if (cities == null)
                return NotFound();
            return cities;
        }

        // GET: api/Cities/5
        [HttpGet("GetCityByCityId/{id}")]
        public async Task<ActionResult<City>> GetCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound();
            return city;
        }

        // GET: api/Cities/5
        [HttpGet("GetAllStates")]
        public async Task<ActionResult<IEnumerable<State>>> GetAllStates()
        {
            var states = await _context.States.ToListAsync();
            if (states == null)
                return NotFound();
            return states;
        }


        // GET: api/Cities/5
        [HttpGet("GetStateByStateId/{id}")]
        public async Task<ActionResult<State>> GetStateByStateId(int id)
        {
            var state = await _context.States.FindAsync(id);
            if (state == null)
                return NotFound();
            return state;
        }


        // GET: api/Cities/5
        [HttpGet("GetAllCountries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            var country = await _context.Country.ToListAsync();
            if (country == null)
                return NotFound();
            return country;
        } 

    }
}
