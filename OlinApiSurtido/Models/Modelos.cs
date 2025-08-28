using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiApi.Models
{
    [Table("DETALLE_DOCUMENTOS")]
    [PrimaryKey(nameof(DOCUMENTO_ID), nameof(ID))]
    public class DetalleDocumento
    {
        public int DOCUMENTO_ID { get; set; }
        public int ID { get; set; }
        public string? NUMERO_DOCUMENTO { get; set; }
        public int TIPO_DOCTO { get; set; }
        public int PRODUCTO { get; set; }
        public string? DESCRIPCION { get; set; }
        public float UNIDADES_SURTIDAS { get; set; }
        public float CANTIDAD { get; set; }
        public int UNIDAD_MEDIDA { get; set; }
        public int UNIDAD_BASE { get; set; }
        public DateTime? FECHA_ENTREGA { get; set; }
    }

    [Table("UNIDADES")]
    public class Unidad
    {
        public int ID { get; set; }
        public string? ABREVIACION { get; set; }
    }

    [Table("PRODUCTOS_PRECIOS")]
    [PrimaryKey(nameof(PRODUCTO), nameof(UNIDAD_MEDIDA_EQUIVALENCIA))]
    public class ProductoPrecio
    {
        public int PRODUCTO { get; set; }
        public int UNIDAD_MEDIDA_EQUIVALENCIA { get; set; }
        public string? CODIGO_BARRAS { get; set; }
    }

    public class GetterDocumentoDetalle
    {
        public int DOCUMENTO_ID { get; set; }
        public required string NUMERO_DOCUMENTO { get; set; }
        public int TIPO_DOCTO { get; set; }
        public int ID { get; set; }
        public int PRODUCTO { get; set; }
        public required string DESCRIPCION { get; set; }
        public float UNIDADES_SURTIDAS { get; set; }
        public float CANTIDAD { get; set; }
        public required string ABREVIACION { get; set; }
        public string? CODIGO_BARRAS { get; set; }
    }
    public class SetterDocumentoDetalle
    {
        public int DOCUMENTO_ID { get; set; }
        public int ID { get; set; }
        public float UNIDADES_SURTIDAS { get; set; }
        public DateTime FECHA_ENTREGA { get; } = DateTime.Now;
    }
}