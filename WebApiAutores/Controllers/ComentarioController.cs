using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libro/{LibroId}/comentario")]
    public class ComentarioController : Controller
    {
        private readonly AplicationContext _context;
        private readonly IMapper _mapper;

        public ComentarioController(AplicationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int LibroId)
        {
            // Verificar si el libro existe por su ID.
            var existeLibro = await _context.Libros.AnyAsync(libroDB => libroDB.Id == LibroId);
            if (!existeLibro) return NotFound();

            // Obtener comentarios asociados al libro y devolverlos como ComentarioDTO.
            var comentarios = await _context.Comentarios.Where(com => com.LibroId == LibroId)
                .ToListAsync();
            return _mapper.Map<List<ComentarioDTO>>(comentarios);
        }


        [HttpPost]
        public async Task<ActionResult> Post([FromRoute] int LibroId, [FromBody] ComentarioCreacionDTO comentarioCreacionDTO)
        {
            // Verificar si el libro existe por su ID.
            var existeLibro = await _context.Libros.AnyAsync(libroDB => libroDB.Id == LibroId);
            if (!existeLibro) return NotFound();

            // Mapear el comentario de ComentarioCreacionDTO y asociarlo al libro.
            var comentario = _mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = LibroId;

            // Agregar el comentario a la base de datos y devolver una respuesta Created con la ubicación del nuevo recurso.
            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();
            return CreatedAtRoute(new { id = comentario.Id }, comentarioCreacionDTO);
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(ComentarioCreacionDTO comentarioDTO, int id)
        {
            // Buscar el comentario por su ID.
            var comentario = await _context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentario == null)
                return NotFound();

            // Mapear el comentario desde ComentarioCreacionDTO y actualizar en la base de datos.
            comentario = _mapper.Map(comentarioDTO, comentario);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
