using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libro")]
    public class LibroController : Controller
    {
        private readonly AplicationContext _context;
        private readonly IMapper _mapper;

        public LibroController(AplicationContext context, IMapper mapper)
        {
            _context = context;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<List<LibroDTO>> Get()
        {
            // Obtener todos los libros con sus comentarios y autores relacionados.
            var libros = await _context.Libros.Include(a => a.Comentarios).Include(b => b.LibrosAutores).ThenInclude(libroAutorDb => libroAutorDb.Autor).ToListAsync();

            // Ordenar los autores de cada libro por su campo "Orden".
            foreach (var item in libros)
            {
                item.LibrosAutores = item.LibrosAutores.OrderBy(x => x.Orden).ToList();
            }

            return _mapper.Map<List<LibroDTO>>(libros);
        }

        [HttpGet("{id:int}", Name = "ObtenerLibro")] // Ruta: api/libro/id
        public async Task<ActionResult<FullLibroDTO>> Get(int id)
        {
            // Verificar si existe un libro con el ID proporcionado.
            var exist = await _context.Libros.AnyAsync(x => x.Id == id);
            if (!exist) return NotFound();

            // Obtener un libro específico con sus comentarios y autores relacionados.
            var libro = await _context.Libros.Include(libroDb => libroDb.Comentarios).Include(x => x.LibrosAutores).ThenInclude(y => y.Autor).FirstOrDefaultAsync(x => x.Id == id);
            libro.LibrosAutores = libro.LibrosAutores.OrderBy(x => x.Orden).ToList();
            return _mapper.Map<FullLibroDTO>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroDTO)
        {
            // Validar los IDs de los autores del libro.
            var autoresIDs = await _context.Autores.Where(autor => libroDTO.AutoresIds.Contains(autor.Id)).Select(x => x.Id).ToListAsync();

            if (libroDTO.AutoresIds.Count() != autoresIDs.Count())
                return BadRequest("Uno o varios de los IDs de los autores no fue encontrado");

            // Mapear y agregar un nuevo libro a la base de datos.
            var libro = _mapper.Map<Libro>(libroDTO);

            if (libro.LibrosAutores != null)
            {
                for (int i = 0; i < libro.LibrosAutores.Count(); i++)
                {
                    libro.LibrosAutores[i].Orden = i;
                }
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            var libroRes = _mapper.Map<LibroDTO>(libro);

            // Devolver la respuesta con el código 201 (Created) y la ubicación del nuevo recurso creado.
            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroRes);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(LibroCreacionDTO libroDTO, int id)
        {
            // Obtener un libro de la base de datos según el ID proporcionado.
            var libroDB = await _context.Libros.Include(al => al.LibrosAutores).FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
                return NotFound();

            // Mapear y actualizar el libro con los datos proporcionados.
            libroDB = _mapper.Map(libroDTO, libroDB);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            // Verificar si existe un libro con el ID proporcionado.
            var exist = await _context.Libros.AnyAsync(x => x.Id == id);

            if (!exist)
                return NotFound();

            // Eliminar el libro de la base de datos.
            var libroEliminado = new Libro() { Id = id };
            _context.Libros.Remove(libroEliminado);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null)
                return BadRequest("Complete los campos de la petición");

            // Obtener un libro de la base de datos según el ID proporcionado.
            var librodb = await _context.Libros.Include(au => au.LibrosAutores).FirstOrDefaultAsync(x => x.Id == id);

            if (librodb == null)
                return NotFound();

            // Mapear el DTO de patch y aplicar las modificaciones al libro.
            var libroDTO = _mapper.Map<LibroPatchDTO>(librodb);
            patchDocument.ApplyTo(libroDTO, ModelState);

            var esValid = TryValidateModel(libroDTO);

            if (!esValid)
                return BadRequest(ModelState);

            // Aplicar las modificaciones al libro en la base de datos y guardar los cambios.
            _mapper.Map(libroDTO, librodb);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
