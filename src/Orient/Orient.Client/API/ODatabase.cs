﻿using System;
using System.Collections.Generic;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;

namespace Orient.Client
{
    public class ODatabase : IDisposable
    {
        private Connection _connection;

        public OSqlCreate Create { get { return new OSqlCreate(_connection); } }

        public ODatabase(string alias)
        {
            _connection = OClient.ReleaseConnection(alias);
        }

        public List<OCluster> GetClusters()
        {
            return _connection.DataObject.Get<List<OCluster>>("Clusters");
        }

        public OSqlSelect Select(string projection)
        {
            OSqlSelect selectQuery = new OSqlSelect(_connection);
            selectQuery.Select(projection);

            return selectQuery;
        }

        public OSqlSelect Select(params string[] projections)
        {
            OSqlSelect selectQuery = new OSqlSelect(_connection);
            selectQuery.Select(projections);

            return selectQuery;
        }

        public List<ORecord> Query(string sql)
        {
            return Query(sql, "*:0");
        }

        public List<ORecord> Query(string sql, string fetchPlan)
        {
            CommandPayload payload = new CommandPayload();
            payload.Type = CommandPayloadType.Sql;
            payload.Text = sql;
            payload.NonTextLimit = -1;
            payload.FetchPlan = fetchPlan;
            payload.SerializedParams = new byte[] { 0 };

            Command operation = new Command();
            operation.OperationMode = OperationMode.Asynchronous;
            operation.ClassType = CommandClassType.Idempotent;
            operation.CommandPayload = payload;

            ODataObject dataObject = _connection.ExecuteOperation<Command>(operation);

            return dataObject.Get<List<ORecord>>("Content");
        }

        public OCommandResult Command(string sql)
        {
            CommandPayload payload = new CommandPayload();
            payload.Type = CommandPayloadType.Sql;
            payload.Text = sql;
            payload.NonTextLimit = -1;
            payload.FetchPlan = "";
            payload.SerializedParams = new byte[] { 0 };

            Command operation = new Command();
            operation.OperationMode = OperationMode.Synchronous;
            operation.ClassType = CommandClassType.NonIdempotent;
            operation.CommandPayload = payload;

            ODataObject dataObject = _connection.ExecuteOperation<Command>(operation);
            
            return new OCommandResult(dataObject);
        }

        public void Close()
        {
            if (_connection.IsReusable)
            {
                OClient.ReturnConnection(_connection);
            }
            else
            {
                _connection.Dispose();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
