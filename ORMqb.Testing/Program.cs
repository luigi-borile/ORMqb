using ORMqb.Compilation;
using ORMqb.Compilation.SqlServer;
using ORMqb.Execution.SqlServer;
using ORMqb.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
#if DEBUG

#else
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;
using System;
#endif

namespace ORMqb.Testing
{
    public class Program
    {
#if DEBUG
        public static async Task Main()
        {
            var builder = new DbSchemaBuilder();
            builder
                .SchemaFor<SaldoTestata>(b => b
                    .Source("dbo.SALDI_TESTATA")
                    .Column(x => x.Stabilimento, "STAB")
                    .Column(x => x.Magazzino, "MAGA")
                    .Column(x => x.Progressivo, "PROG"))
                .SchemaFor<SaldoDettaglio>(b => b
                    .Source("dbo.SALDI_DETTAGLIO")
                    .Column(x => x.Stabilimento, "STAB")
                    .Column(x => x.Magazzino, "MAGA")
                    .Column(x => x.Progressivo, "PROG")
                    .Column(x => x.Articolo, "ARTI")
                    .Column(x => x.IdLotto, "IdLotto")
                    .Column(x => x.QtPezzi, "QT_PEZZI"))
                .SchemaFor<Articolo>(b => b
                    .Source("dbo.ARTICOLI")
                    .Column(x => x.CodiceArticolo, "ARTI")
                    .Column(x => x.Descrizione, "DSCR")
                    .Column(x => x.TipoArticolo, "TIPO_ARTI"));

            builder
                .StoredProcedure<SpGetProgressivo>(b => b
                    .Source("dbo.GetProgressivo")
                    .Parameter(x => x.Stabilimento, "@stab")
                    .Parameter(x => x.Magazzino, "@maga")
                    .Parameter(x => x.Anno, "@anno")
                    .Parameter(x => x.Caso, "@caso")
                    .OutputParameter(x => x.Progressivo, "@progressivo")
                    .OutputParameter(x => x.Errore, "@errore"));

            IQueryCompiler compiler = new QueryCompiler(builder);
            IQueryExecutor executor = new QueryExecutor("Data Source =.\\; Initial Catalog = EasyStock_Dev; User ID = sa; Password = m4dl4b2013;");
            IQueryFactory factory = new QueryFactory(compiler, executor);

            //var sp = new SpGetProgressivo
            //{
            //    Stabilimento = "MAD",
            //    Magazzino = "000",
            //    Anno = 2020,
            //    Caso = "PLT"
            //};
            //int resultCode = await factory.ExecAsync(sp);

            //IEnumerable<Result2> result1 = await factory.GetManyAsync<Result2>(q => q
            //    .From<SaldoDettaglio>(sd => sd)
            //    .Select<SaldoDettaglio, decimal>(sd => SqlFn.Sum(() => sd.QtPezzi), r => r.Sum)
            //    .Select<SaldoDettaglio, decimal>(sd => SqlFn.Average(() => sd.QtPezzi), r => r.Cnt)
            //    .Select<SaldoDettaglio, decimal>(sd => SqlFn.Min(() => sd.QtPezzi), r => r.Min)
            //    .Select<SaldoDettaglio, decimal>(sd => SqlFn.Max(() => sd.QtPezzi), r => r.Max)
            //);

            //IEnumerable<Result> ress = await factory.GetManyAsync<Result>(q => q
            //    .From<SaldoDettaglio>(sd => sd)
            //    .Select<SaldoDettaglio, string>(sd => sd.Stabilimento, r => r.Stabilimento)
            //    .Select<SaldoDettaglio, string>(sd => sd.Magazzino, r => r.Magazzino)
            //    .Select<SaldoDettaglio, string>(sd => sd.Progressivo, r => r.Progressivo)
            //    .Select<SaldoDettaglio, decimal>(sd => sd.QtPezzi, r => r.QtPezzi));

            IEnumerable<Result2> result2 = await factory.GetManyAsync<Result2>(q => q
                .Select<SaldoTestata, string>(st => st.Stabilimento, r => r.Stabilimento)
                .Select<SaldoTestata, string>(st => st.Magazzino, r => r.Magazzino)
                .Select<SaldoTestata, int>(st => SqlFn.Count(() => st.Progressivo), r => r.Cnt)
                .Select<SaldoTestata, long>(st => SqlFn.CountBig(() => st.Progressivo), r => r.CntBig)
                .From<SaldoTestata>(st => st)
                .Where<SaldoTestata>(w => !SqlFn.Exists(e => e
                    .Select(1)
                    .From<SaldoDettaglio>(sd => sd)
                    .Where<SaldoTestata, SaldoDettaglio>((st, sd) =>
                        st.Stabilimento == sd.Stabilimento &&
                        st.Magazzino == sd.Magazzino &&
                        st.Progressivo == sd.Progressivo)
                    )
                )
                .GroupBy<SaldoTestata>(st => st.Stabilimento)
                .GroupBy<SaldoTestata>(st => st.Magazzino)
                .OrderByDesc<SaldoTestata>(st => st.Stabilimento)
                .OrderBy<SaldoTestata>(st => st.Magazzino)
            );

            //IEnumerable<Result2> result = await factory.GetManyAsync<Result2>(q => q
            //    .Select<Result, string>(r => r.Stabilimento, r => r.Progressivo)
            //    .Select<Result, int>(r => Convert.ToInt32(r.Magazzino), r => r.QtPezzi)
            //    .FromQuery<Result>(qInterna => qInterna
            //        .Select<SaldoTestata, string>(st => st.Stabilimento + "X", r => r.Stabilimento)
            //        .From<SaldoTestata>(st => st)
            //        .Join<SaldoTestata, SaldoDettaglio>((st, sd) => st.Stabilimento == sd.Magazzino || sd.Magazzino != st.Magazzino)
            //        .Where<SaldoDettaglio>((w, sd) => sd.IdLotto != 1)));
        }
#else
        public static void Main()
        {
            BenchmarkRunner.Run(
                typeof(Program).Assembly,
                DefaultConfig.Instance
                .WithOption(ConfigOptions.DisableOptimizationsValidator, true)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddDiagnoser(new NativeMemoryProfiler()));

            Console.ReadLine();
        }
#endif



        private class Result
        {
            public string Stabilimento { get; set; }
            public string Magazzino { get; set; }
            public string Progressivo { get; set; }
            public decimal QtPezzi { get; set; }
        }

        private class Result2
        {
            public string Stabilimento { get; set; }
            public string Magazzino { get; set; }
            public int Cnt { get; set; }
            public long CntBig { get; set; }
            public decimal Sum { get; set; }
            public decimal Min { get; set; }
            public decimal Max { get; set; }
        }
    }
}
