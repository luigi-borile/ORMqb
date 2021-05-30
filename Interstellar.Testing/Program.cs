#if DEBUG
using Interstellar.Compilation;
using Interstellar.Schema;
using Interstellar.SqlServer;
using System.Collections.Generic;
using System.Threading.Tasks;
#else
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;
#endif

namespace Interstellar.Testing
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
                .ForeignKey<SaldoTestata, SaldoDettaglio>(b => b
                    .Column(sd => sd.Stabilimento, st => st.Stabilimento)
                    .Column(sd => sd.Magazzino, st => st.Magazzino)
                    .Column(sd => sd.Progressivo, st => st.Progressivo))
                .ForeignKey<Articolo, SaldoDettaglio>(b => b
                    .Column(a => a.CodiceArticolo, sd => sd.Articolo));

            IQueryCompiler compiler = new QueryCompiler(builder);
            IQueryExecutor executor = new QueryExecutor("Data Source =.\\; Initial Catalog = EasyStock_Dev; User ID = sa; Password = m4dl4b2013;");
            IQueryFactory factory = new QueryFactory(compiler, executor);

            IEnumerable<Result> result = await factory.GetManyAsync<Result>(q => q
            .Select<Result, string>(inner => inner.Progressivo, r => r.Progressivo)
            .FromQuery<Result>(inner => inner
                .Select<SaldoTestata, string>(st => "Stab:" + st.Stabilimento, r => r.Stabilimento)
                .Select<SaldoTestata, string>(st => "Maga:" + st.Magazzino, r => r.Magazzino)
                .Select<SaldoTestata, string>(st => st.Progressivo, r => r.Progressivo)
                .Select<SaldoDettaglio, decimal>(sd => sd.QtPezzi, r => r.QtPezzi)
                .From<SaldoTestata>(st => st)
                .Join<SaldoTestata, SaldoDettaglio>((st, sd) =>
                    st.Stabilimento == sd.Stabilimento &&
                    st.Magazzino == sd.Magazzino &&
                    st.Progressivo == sd.Progressivo)
                .Where<SaldoDettaglio>(sd => sd.IdLotto != 1)
                )
            );


            IEnumerable<Result> result2 = await factory.GetManyAsync<Result>(q => q
                .Select<SaldoTestata, string>(st => st.Progressivo)
                .From<SaldoTestata>(st => st)
                .Where<SaldoTestata>(w => SqlFunctions.Exists(e => e
                    .SelectValue(1)
                    .From<SaldoDettaglio>(sd => sd)
                    .Where<SaldoTestata, SaldoDettaglio>((st, sd) =>
                        st.Stabilimento == sd.Stabilimento &&
                        st.Magazzino == sd.Magazzino &&
                        st.Progressivo == sd.Progressivo)
                    )
                )
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
        }
    }
}
