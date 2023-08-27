using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Mapeo entre AutorCreacionDTO y Autor
            CreateMap<AutorCreacionDTO, Autor>();

            // Mapeo entre Autor y AutorDTO
            CreateMap<Autor, AutorDTO>();

            // Mapeo entre Autor y FullAutorDTO, donde se realiza mapeo personalizado para la lista de libros
            CreateMap<Autor, FullAutorDTO>().ForMember(autor => autor.Libros, opc => opc.MapFrom(MapAutorAutor));

            // Mapeo entre LibroCreacionDTO y Libro, con mapeo personalizado para la lista de autores
            CreateMap<LibroCreacionDTO, Libro>().ForMember(libro => libro.LibrosAutores, opc => opc.MapFrom(MapLibroAutor));

            // Mapeo entre Libro y LibroDTO
            CreateMap<Libro, LibroDTO>();

            // Mapeo entre Libro y FullLibroDTO, con mapeo personalizado para la lista de autores
            CreateMap<Libro, FullLibroDTO>().ForMember(libro => libro.Autores, opc => opc.MapFrom(MapLibroLibroDTO));

            // Mapeo entre LibroPatchDTO y Libro, con mapeo personalizado para la lista de autores
            CreateMap<LibroPatchDTO, Libro>().ForMember(Libro => Libro.LibrosAutores, opc => opc.MapFrom(MapLibroPatch)).ReverseMap();

            // Mapeo entre ComentarioCreacionDTO y Comentario
            CreateMap<ComentarioCreacionDTO, Comentario>();

            // Mapeo entre Comentario y ComentarioDTO
            CreateMap<Comentario, ComentarioDTO>();
        }

        // Función de mapeo personalizado para LibroPatchDTO y Libro, específicamente para la lista de autores
        private List<LibroAutor> MapLibroPatch(LibroPatchDTO dTO, Libro libro)
        {
            var listaLibroAutor = new List<LibroAutor>();

            // Verifica si la lista de IDs de autores en LibroPatchDTO es nula
            if (dTO.AutoresIds == null)
                return listaLibroAutor;

            // Itera a través de los IDs de autores en LibroPatchDTO
            foreach (var item in dTO.AutoresIds)
            {
                // Crea objetos LibroAutor y asigna el ID del autor correspondiente
                listaLibroAutor.Add(new LibroAutor() { AutorId = item });
            }

            return listaLibroAutor;
        }


        // Función de mapeo personalizado para relacionar un autor con sus libros en AutorDTO
        private List<LibroDTO> MapAutorAutor(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();

            // Verifica si la lista de LibrosAutores en el objeto Autor es nula
            if (autor.LibrosAutores == null)
                return resultado;

            // Itera a través de los objetos LibroAutor en la lista LibrosAutores del autor
            foreach (var item in autor.LibrosAutores)
            {
                // Agrega un nuevo objeto LibroDTO a la lista resultado, utilizando los datos de LibroAutor
                resultado.Add(new LibroDTO() { Id = item.LibroId, Titulo = item.Libro.Titulo });
            }

            return resultado;
        }


        // Función de mapeo personalizado para relacionar un libro con sus autores en FullLibroDTO
        private List<AutorDTO> MapLibroLibroDTO(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorDTO>();

            // Verifica si la lista de LibrosAutores en el objeto Libro es nula
            if (libro.LibrosAutores == null)
            {
                return resultado;
            }

            // Itera a través de los objetos LibroAutor en la lista LibrosAutores del libro
            foreach (var item in libro.LibrosAutores)
            {
                // Agrega un nuevo objeto AutorDTO a la lista resultado, utilizando los datos de LibroAutor
                resultado.Add(new AutorDTO() { Id = item.AutorId, Nombre = item.Autor.Nombre });
            }

            return resultado;
        }


        // Función de mapeo personalizado para relacionar autores con un libro en LibroCreacionDTO
        private List<LibroAutor> MapLibroAutor(LibroCreacionDTO dTO, Libro libro)
        {
            var listaLibroAutor = new List<LibroAutor>();

            // Verifica si la lista de AutoresIds en el objeto LibroCreacionDTO es nula
            if (dTO.AutoresIds == null)
                return listaLibroAutor;

            // Itera a través de los IDs de autores en la lista AutoresIds del objeto LibroCreacionDTO
            foreach (var item in dTO.AutoresIds)
            {
                // Crea un nuevo objeto LibroAutor y lo agrega a la lista listaLibroAutor
                listaLibroAutor.Add(new LibroAutor() { AutorId = item });
            }

            return listaLibroAutor;
        }


    }
}
