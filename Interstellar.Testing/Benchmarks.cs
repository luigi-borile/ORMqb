using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Interstellar.Schema;
using Interstellar.SqlServer;
using SqlKata.Compilers;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Interstellar.Testing
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50, baseline: true)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    //[SimpleJob(RuntimeMoniker.Net48)]
    public class Benchmarks
    {
        private QueryFactory _factory;
        private SqlKata.Execution.QueryFactory _kataFactory;

        [GlobalSetup]
        public void Setup()
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

            var compiler = new QueryCompiler(builder);
            var executor = new QueryExecutor("Data Source =.\\; Initial Catalog = EasyStock_Dev; User ID = sa; Password = m4dl4b2013;");
            _factory = new QueryFactory(compiler, executor);

            _kataFactory = new SqlKata.Execution.QueryFactory(
                new SqlConnection("Data Source =.\\; Initial Catalog = EasyStock_Dev; User ID = sa; Password = m4dl4b2013;"),
                new SqlServerCompiler());
        }

        [Benchmark]
        public async Task QueryManyAsync()
        {
            IEnumerable<Result> result = await _factory.GetManyAsync<Result>(q => q
            .Select<Result, string>(inner => inner.Stabilimento, r => r.Stabilimento)
            .Select<Result, string>(inner => inner.Magazzino, r => r.Magazzino)
            .Select<Result, string>(inner => inner.Progressivo, r => r.Progressivo)
            .Select<Result, decimal>(inner => inner.QtPezzi, r => r.QtPezzi)
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
        }

        [Benchmark]
        public async Task QueryManySqlKataAsync()
        {
            IEnumerable<Result> result =
               await _kataFactory.GetAsync<Result>(
                   new SqlKata.Query()
                   .Select(
                       $"inner.Stabilimento AS {nameof(Result.Stabilimento)}",
                       $"inner.Magazzino AS {nameof(Result.Magazzino)}",
                       $"inner.Progressivo AS {nameof(Result.Progressivo)}",
                       $"inner.QtPezzi AS {nameof(Result.QtPezzi)}")
                   .From(
                       new SqlKata.Query()
                       .As("inner")
                       .Select(
                           $"st.STAB AS {nameof(Result.Stabilimento)}",
                           $"st.MAGA AS {nameof(Result.Magazzino)}",
                           $"st.PROG AS {nameof(Result.Progressivo)}",
                           $"sd.QT_PEZZI AS {nameof(Result.QtPezzi)}")
                       .From("dbo.SALDI_TESTATA AS st")
                       .Join("dbo.SALDI_DETTAGLIO AS sd", j => j.On("st.STAB", "sd.STAB").On("st.MAGA", "sd.MAGA").On("st.PROG", "sd.PROG"))
                       .Where("sd.IdLotto", "<>", 1)))
               .ConfigureAwait(false);
        }

        private class Result
        {
            public string Stabilimento { get; set; }
            public string Magazzino { get; set; }
            public string Progressivo { get; set; }
            public decimal QtPezzi { get; set; }
        }
    }
}
