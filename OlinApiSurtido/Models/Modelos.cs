using System.ComponentModel.DataAnnotations.Schema;

namespace MiApi.Models
{
    // --- Entities ---

    [Table("DETALLE_DOCUMENTOS")]
    public class DetalleDocumento
    {
        public int DOCUMENTO_ID { get; set; }
        public int ID { get; set; }
        public string? NUMERO_DOCUMENTO { get; set; }
        public int TIPO_DOCTO { get; set; }
        public int? PRODUCTO { get; set; }
        public string? DESCRIPCION { get; set; }
        public float CANTIDAD { get; set; }
        public int? UNIDAD_MEDIDA { get; set; }
        public int UNIDAD_BASE { get; set; }

        public virtual SurtidoDetalle? SurtidoDetalle { get; set; }
    }

    [Table("UNIDADES")]
    public class Unidad
    {
        public int ID { get; set; }
        public string? ABREVIACION { get; set; }
    }

    [Table("PRODUCTOS_PRECIOS")]
    public class ProductoPrecio
    {
        public int PRODUCTO { get; set; }
        public int UNIDAD_MEDIDA_EQUIVALENCIA { get; set; }
        public string? CODIGO_BARRAS { get; set; }
    }

    [Table("SURTIDOS_ENCABEZADO")]
    public class SurtidoEncabezado
    {
        public int ID { get; set; }
        public bool COMPLETADO { get; set; }
    }

    [Table("SURTIDOS_DETALLE")]
    public class SurtidoDetalle
    {
        public int ID { get; set; }
        public float? SURTIDAS { get; set; }
        public int? CHECADOR { get; set; }
        public DateTime? FIN_SURTIDO { get; set; }

        public virtual DetalleDocumento? DetalleDocumento { get; set; }
    }

    // --- DTOs ---

    // DTO for the GET endpoint response, matches the user's requested SELECT query
    public class GetterDocumentoDetalle
    {
        public int ID { get; set; }
        public int PRODUCTO { get; set; }
        public string DESCRIPCION { get; set; } = string.Empty;
        public float? SURTIDAS { get; set; } // Nullable because of LEFT JOIN
        public float CANTIDAD { get; set; }
        public string? ABREVIACION { get; set; } // Nullable to precisely match JOIN results
        public string? CODIGO_BARRAS { get; set; }
    }

    // DTO for the PATCH endpoint request
    public class SetterSurtidos_Detalle
    {
        public int ID { get; set; }
        public float SURTIDAS { get; set; }
        public int CHECADOR { get; set; }
    }
}