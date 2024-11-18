namespace ProvaLPI.Domain
{
    public class Perfil
    {
        public int PerfilId { get; set; }
        public string Nome { get; set; }
        public PerfilTipo Tipo { get; set; }
        public int UsuarioId { get; set; }
    }
}
