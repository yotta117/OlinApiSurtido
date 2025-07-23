namespace MiApi.Models
{
    public class DocumentoDetalle
    {
        public int DOCUMENTO_ID { get; set; }
        public required string NUMERO_DOCUMENTO { get; set; }
        public int TIPO_DOCTO { get; set; }
        public int ID { get; set; }
        public int PRODUCTO { get; set; }
        public required string DESCPRIPCION { get; set; }
        public float UNIDADES_SURTIDAS { get; set; }
        public float CANTIDAD { get; set; }
        public required string ABREVIACION { get; set; }
        public string? CODIGO_BARRAS { get; set; }
        public DateTime FECHA_ENTREGA { get; } = DateTime.Now;
        public bool isVisible { get; } = true;
    }
}