using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using naturalGlow.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace naturalGlow.UTIL
{
    public static class GeneradorFactura
    {
        #region Generar Factura

        public static string GenerarFacturaPDF(
            Factura factura,
            Comanda comanda,
            User client,
            List<LiniaComanda> linies,
            List<Devolucio> devolucions,
            List<Producte> productes)
        {
            string rutaPDF =
                PrepararRutaPDF(
                    factura);

            ConfigurarQuestPDF();

            CrearDocument(
                rutaPDF,
                factura,
                comanda,
                client,
                linies,
                devolucions,
                productes);

            return rutaPDF;
        }

        #endregion

        #region Preparar Ruta PDF

        private static string PrepararRutaPDF(
            Factura factura)
        {
            string carpetaFactures =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments),
                    "naturalGlow",
                    "Factures");

            Directory.CreateDirectory(
                carpetaFactures);

            string nomFitxer =
                "Factura_"
                + factura.Id
                + ".pdf";

            string rutaPDF =
                Path.Combine(
                    carpetaFactures,
                    nomFitxer);

            return rutaPDF;
        }

        #endregion

        #region Configurar QuestPDF

        private static void ConfigurarQuestPDF()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;
        }

        #endregion

        #region Crear Document

        private static void CrearDocument(
            string rutaPDF,
            Factura factura,
            Comanda comanda,
            User client,
            List<LiniaComanda> linies,
            List<Devolucio> devolucions,
            List<Producte> productes)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(
                        page,
                        factura,
                        comanda,
                        client,
                        linies,
                        devolucions,
                        productes);
                });
            })
            .GeneratePdf(
                rutaPDF);
        }

        #endregion

        #region Configurar Pagina

        private static void ConfigurarPagina(
            PageDescriptor page,
            Factura factura,
            Comanda comanda,
            User client,
            List<LiniaComanda> linies,
            List<Devolucio> devolucions,
            List<Producte> productes)
        {
            page.Margin(40);

            CrearCapcalera(
                page);

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    CrearDadesFactura(
                        column,
                        factura,
                        comanda);

                    CrearDadesClient(
                        column,
                        client);

                    CrearProductes(
                        column,
                        linies,
                        devolucions,
                        productes);

                    CrearDevolucions(
                        column,
                        devolucions,
                        productes);

                    CrearTotals(
                        column,
                        factura,
                        comanda);
                });

            CrearPeuPagina(
                page);
        }

        #endregion

        #region Capcalera

        private static void CrearCapcalera(
            PageDescriptor page)
        {
            page.Header()
                .Text("naturalGlow")
                .FontSize(24)
                .Bold();
        }

        #endregion

        #region Dades Factura

        private static void CrearDadesFactura(
            ColumnDescriptor column,
            Factura factura,
            Comanda comanda)
        {
            column.Item()
                .Text("FACTURA")
                .FontSize(20)
                .Bold();

            column.Item()
                .Text(
                    "Número de factura: "
                    + factura.Id);

            column.Item()
                .Text(
                    "Data: "
                    + factura.Data
                        .ToLocalTime()
                        .ToString(
                            "dd/MM/yyyy HH:mm"));

            column.Item()
                .Text(
                    "Comanda: "
                    + factura.IdComanda);

            column.Item()
                .Background(
                    Colors.Grey.Lighten3)
                .Padding(8)
                .Text(
                    "ESTAT DE LA COMANDA: "
                    + comanda.Estat)
                .FontSize(14)
                .Bold();

            column.Item()
                .LineHorizontal(1);
        }

        #endregion

        #region Dades Client

        private static void CrearDadesClient(
            ColumnDescriptor column,
            User client)
        {
            column.Item()
                .Text("CLIENT")
                .FontSize(16)
                .Bold();

            column.Item()
                .Text(
                    "Nom: "
                    + client.Nom);

            column.Item()
                .Text(
                    "Email: "
                    + client.Email);

            column.Item()
                .Text(
                    "Telèfon: "
                    + client.Telefon);

            column.Item()
                .Text(
                    "Adreça: "
                    + client.Adreca);

            column.Item()
                .LineHorizontal(1);
        }

        #endregion

        #region Productes

        private static void CrearProductes(
            ColumnDescriptor column,
            List<LiniaComanda> linies,
            List<Devolucio> devolucions,
            List<Producte> productes)
        {
            column.Item()
                .Text("PRODUCTES")
                .FontSize(16)
                .Bold();

            foreach (
                LiniaComanda linia
                in linies)
            {
                Producte producte =
                    productes.FirstOrDefault(
                        p =>
                            p.Id ==
                            linia.IdProducte);

                if (producte == null)
                {
                    continue;
                }

                int quantitatRetornada =
                    CalcularQuantitatRetornada(
                        linia,
                        devolucions);

                int quantitatActual =
                    CalcularQuantitatActual(
                        linia,
                        quantitatRetornada);

                double importOriginal =
                    producte.PreuVenda *
                    linia.Quantitat;

                double importRetornat =
                    producte.PreuVenda *
                    quantitatRetornada;

                double importActual =
                    producte.PreuVenda *
                    quantitatActual;

                CrearLiniaProducte(
                    column,
                    producte,
                    linia,
                    quantitatRetornada,
                    quantitatActual,
                    importOriginal,
                    importRetornat,
                    importActual);
            }
        }

        #endregion

        #region Calcular Quantitat Retornada

        private static int CalcularQuantitatRetornada(
            LiniaComanda linia,
            List<Devolucio> devolucions)
        {
            int quantitatRetornada =
                devolucions
                    .Where(
                        d =>
                            d.IdProducte ==
                            linia.IdProducte &&
                            d.Estat ==
                            "Acceptada")
                    .Sum(
                        d =>
                            d.Quantitat);

            return quantitatRetornada;
        }

        #endregion

        #region Calcular Quantitat Actual
        private static int CalcularQuantitatActual(
            LiniaComanda linia,
            int quantitatRetornada)
        {
            int quantitatActual =
                linia.Quantitat -
                quantitatRetornada;

            if (quantitatActual < 0)
            {
                quantitatActual = 0;
            }

            return quantitatActual;
        }

        #endregion

        #region Crear Linia Producte

        private static void CrearLiniaProducte(
            ColumnDescriptor column,
            Producte producte,
            LiniaComanda linia,
            int quantitatRetornada,
            int quantitatActual,
            double importOriginal,
            double importRetornat,
            double importActual)
        {
            column.Item()
                .Text(
                    producte.Nom);

            column.Item()
                .Text(
                    "Comprat: "
                    + linia.Quantitat
                    + " x "
                    + producte.PreuVenda
                        .ToString("F2")
                    + " €"
                    + " = "
                    + importOriginal
                        .ToString("F2")
                    + " €");

            if (quantitatRetornada > 0)
            {
                column.Item()
                    .Text(
                        "Devolució: "
                        + quantitatRetornada
                        + " unitat(s)"
                        + " = -"
                        + importRetornat
                            .ToString("F2")
                        + " €");
            }

            column.Item()
                .Text(
                    "Quantitat actual: "
                    + quantitatActual
                    + "    Total: "
                    + importActual
                        .ToString("F2")
                    + " €");

            column.Item()
                .LineHorizontal(0.5f);
        }

        #endregion

        #region Devolucions

        private static void CrearDevolucions(
            ColumnDescriptor column,
            List<Devolucio> devolucions,
            List<Producte> productes)
        {
            if (devolucions != null &&
                devolucions.Any(
                    d =>
                        d.Estat ==
                        "Acceptada"))
            {
                column.Item()
                    .PaddingTop(10)
                    .Text("DEVOLUCIONS")
                    .FontSize(16)
                    .Bold();

                foreach (
                    Devolucio devolucio
                    in devolucions.Where(
                        d =>
                            d.Estat ==
                            "Acceptada"))
                {
                    CrearLiniaDevolucio(
                        column,
                        devolucio,
                        productes);
                }
            }
        }

        #endregion

        #region Linia Devolucio

        private static void CrearLiniaDevolucio(
            ColumnDescriptor column,
            Devolucio devolucio,
            List<Producte> productes)
        {
            Producte producte =
                productes.FirstOrDefault(
                    p =>
                        p.Id ==
                        devolucio.IdProducte);

            if (producte == null)
            {
                return;
            }

            double importDevolucio =
                producte.PreuVenda *
                devolucio.Quantitat;

            column.Item()
                .Text(
                    "Producte: "
                    + producte.Nom);

            column.Item()
                .Text(
                    "Quantitat retornada: "
                    + devolucio.Quantitat);

            column.Item()
                .Text(
                    "Import retornat: "
                    + importDevolucio
                        .ToString("F2")
                    + " €");

            column.Item()
                .Text(
                    "Data: "
                    + devolucio.Data
                        .ToLocalTime()
                        .ToString(
                            "dd/MM/yyyy HH:mm"));

            column.Item()
                .LineHorizontal(0.5f);
        }

        #endregion

        #region Totals

        private static void CrearTotals(
            ColumnDescriptor column,
            Factura factura,
            Comanda comanda)
        {
            column.Item()
                .PaddingTop(10)
                .LineHorizontal(1);

            column.Item()
                .AlignRight()
                .Text(
                    "TOTAL FINAL: "
                    + factura.Total
                        .ToString("F2")
                    + " €")
                .FontSize(18)
                .Bold();

            column.Item()
                .AlignRight()
                .Text(
                    "Estat actual: "
                    + comanda.Estat)
                .FontSize(12)
                .Bold();
        }

        #endregion

        #region Peu Pagina

        private static void CrearPeuPagina(
            PageDescriptor page)
        {
            page.Footer()
                .AlignCenter()
                .Text(
                    "Gràcies per comprar a naturalGlow");
        }

        #endregion
    }
}