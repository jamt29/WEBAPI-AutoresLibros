namespace WebApiAutores.DTOs
{
    public class FullLibroDTO: LibroDTO
    {

        public IEnumerable<ComentarioDTO> Comentarios { get; set; }
        public IEnumerable<AutorDTO> Autores { get; set; }
    }
}
