using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Client.Models;

namespace Client.Services
{
    public class SoftwareSearchService
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private readonly RAMDirectory _directory;
        private readonly StandardAnalyzer _analyzer;
        private readonly List<Software> _originalList;

        public SoftwareSearchService()
        {
            _directory = new RAMDirectory();
            _analyzer = new StandardAnalyzer(AppLuceneVersion);
            _originalList = new List<Software>();
        }

        public void UpdateSearchIndex(IEnumerable<Software> softwares)
        {
            _originalList.Clear();
            _originalList.AddRange(softwares);

            using var writer = new IndexWriter(_directory, new IndexWriterConfig(AppLuceneVersion, _analyzer));
            writer.DeleteAll();

            foreach (var software in softwares)
            {
                var doc = new Document
                {
                    new TextField("name", software.Name ?? "", Field.Store.YES),
                    new TextField("description", software.Description ?? "", Field.Store.YES),
                    new StringField("id", software.Name ?? "", Field.Store.YES) // 使用名称作为ID
                };
                writer.AddDocument(doc);
            }

            writer.Commit();
        }

        public List<Software> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _originalList.ToList();
            }

            using var reader = DirectoryReader.Open(_directory);
            var searcher = new IndexSearcher(reader);

            var nameQuery = new FuzzyQuery(new Term("name", keyword.ToLower()), 2);
            var descQuery = new FuzzyQuery(new Term("description", keyword.ToLower()), 2);

            var combinedQuery = new BooleanQuery
            {
                { nameQuery, Occur.SHOULD },
                { descQuery, Occur.SHOULD }
            };

            var hits = searcher.Search(combinedQuery, 50).ScoreDocs;
            
            var results = new List<Software>();
            foreach (var hit in hits)
            {
                var doc = searcher.Doc(hit.Doc);
                var software = _originalList.FirstOrDefault(s => s.Name == doc.Get("id"));
                if (software != null)
                {
                    results.Add(software);
                }
            }

            return results;
        }
    }
} 
