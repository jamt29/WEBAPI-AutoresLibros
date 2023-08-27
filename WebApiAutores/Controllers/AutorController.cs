using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autor")]
    public class AutorController : Controller
    {
        private readonly AplicationContext _context;
        private readonly IMapper _mapper;

        public AutorController(AplicationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<AutorDTO>>> Get()
        {
            // Obtener todos los autores con sus relaciones de libros.
            var autores = await _context.Autores.Include(x => x.LibrosAutores).ThenInclude(y => y.Libro).ToListAsync();
            return _mapper.Map<List<AutorDTO>>(autores);
        }


        [HttpGet("{id:int}", Name = "ObtenerAutor")]
        public async Task<ActionResult<FullAutorDTO>> Get([FromRoute] int id)
        {
            // Obtener un autor específico por su ID junto con sus relaciones de libros.
            var autor = await _context.Autores.Include(x => x.LibrosAutores).ThenInclude(y => y.Libro).FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null) return NotFound();
            return _mapper.Map<FullAutorDTO>(autor);
        }


        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<FullAutorDTO>>> Get([FromRoute] string nombre)
        {
            // Obtener autores que contengan el nombre especificado, con sus relaciones de libros.
            var autores = await _context.Autores.Include(x => x.LibrosAutores).ThenInclude(y => y.Libro)
                          .Where(autor => autor.Nombre.Contains(nombre)).ToListAsync();
            if (!autores.Any()) return NotFound();
            return _mapper.Map<List<FullAutorDTO>>(autores);
        }


        [HttpPost]
        public async Task<IActionResult> Post(AutorCreacionDTO autorDTO)
        {
            // Mapear y agregar un nuevo autor a la base de datos.
            var autor = _mapper.Map<Autor>(autorDTO);
            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            // Mapear el autor creado a AutorDTO y devolver una respuesta Created con la ubicación del recurso.
            var autorRes = _mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new { id = autor.Id }, autorRes);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(AutorCreacionDTO autorDTO, int id)
        {
            // Verificar si el autor existe por su ID.
            var exist = await _context.Autores.AnyAsync(x => x.Id == id);
            if (!exist) return NotFound();

            // Mapear el DTO a la entidad, establecer el ID y actualizar en la base de datos.
            var autor = _mapper.Map<Autor>(autorDTO);
            autor.Id = id;
            _context.Autores.Update(autor);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Verificar si el autor existe por su ID.
            var exist = await _context.Autores.AnyAsync(x => x.Id == id);
            if (!exist) return NotFound();

            // Eliminar el autor de la base de datos.
            var autorEliminado = new Autor() { Id = id };
            _context.Autores.Remove(autorEliminado);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
