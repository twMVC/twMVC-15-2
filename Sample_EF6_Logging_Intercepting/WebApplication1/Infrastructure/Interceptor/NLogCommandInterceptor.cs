using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using NLog;

namespace WebApplication1.Infrastructure.Interceptor
{
    public class NLogCommandInterceptor : IDbCommandInterceptor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Stopwatch _stopwatch = new Stopwatch();

        protected internal Stopwatch Stopwatch
        {
            get { return _stopwatch; }
        }


        public void NonQueryExecuting(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogIfNonAsync(command, interceptionContext);
            Stopwatch.Restart();
        }

        public void NonQueryExecuted(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Stopwatch.Stop();
            LogNonQueryCommand(command, interceptionContext);
        }

        public void ReaderExecuting(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            //LogIfNonAsync(command, interceptionContext);
            Stopwatch.Restart();
        }

        public void ReaderExecuted(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            if (interceptionContext.Exception != null)
            {
                LogIfError(command, interceptionContext);
            }
            else
            {
                Stopwatch.Stop();
                string logContent = LogCommand(command, interceptionContext);
                Logger.Trace(logContent);
            }
        }

        public void ScalarExecuting(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //LogIfNonAsync(command, interceptionContext);
            Stopwatch.Restart();
        }

        public void ScalarExecuted(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            if (interceptionContext.Exception != null)
            {
                LogIfError(command, interceptionContext);
            }
            else
            {
                Stopwatch.Stop();
                string logContent = LogCommand(command, interceptionContext);
                Logger.Trace(logContent);
            }
        }

        //=========================================================================================

        private void LogIfNonAsync<TResult>(
            DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (!interceptionContext.IsAsync)
            {
                Logger.Trace("Non-async command used: {0}", command.CommandText);
            }
        }

        private void LogIfError<TResult>(
            DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (interceptionContext.Exception != null)
            {
                Logger.Error("Command {0} failed with exception {1}",
                    command.CommandText, interceptionContext.Exception);
            }
        }

        private void LogNonQueryCommand<TResult>(
            DbCommand command,
            DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (interceptionContext == null)
            {
                throw new ArgumentNullException("interceptionContext");
            }

            if (interceptionContext.Exception != null)
            {
                LogIfError(command, interceptionContext);
            }
            else
            {
                //只有針對不是非同步處理
                if (!interceptionContext.IsAsync)
                {
                    string logContent = LogCommand(command, interceptionContext);
                    Logger.Trace(logContent);
                }
            }
        }

        /// <summary>
        /// Logs the command.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="interceptionContext">The interception context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// command
        /// or
        /// interceptionContext
        /// </exception>
        private string LogCommand<TResult>(
            DbCommand command,
            DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (interceptionContext == null)
            {
                throw new ArgumentNullException("interceptionContext");
            }

            var commandText = command.CommandText ?? "<null>";

            if (!commandText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                commandText = string.Concat(commandText, Environment.NewLine);
            }

            commandText = string.Concat(
                commandText, Environment.NewLine,
                "Parameters: ", Environment.NewLine);

            foreach (var parameter in command.Parameters.OfType<DbParameter>())
            {
                commandText = string.Concat(
                    commandText,
                    LogParameter(command, interceptionContext, parameter));
            }

            //執行開始時間
            commandText = string.Concat(commandText, Environment.NewLine,
                "-- Executing at ", DateTimeOffset.Now.ToLocalTime());

            //取得目前執行個體所測量的已耗用時間總和，以毫秒為單位

            long z = 10000 * Stopwatch.ElapsedTicks;
            decimal timesTen = (int)(z / Stopwatch.Frequency);
            var elapsedMilliseconds = timesTen / 10;

            commandText = string.Concat(commandText, Environment.NewLine,
                "-- Completed in ", elapsedMilliseconds, " ms");

            return commandText;
        }

        private string LogParameter<TResult>(
            DbCommand command,
            DbCommandInterceptionContext<TResult> interceptionContext,
            DbParameter parameter)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (interceptionContext == null)
            {
                throw new ArgumentNullException("interceptionContext");
            }
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            // -- Name: [Value] (Type = {}, Direction = {}, IsNullable = {}, Size = {}, Precision = {} Scale = {})
            var builder = new StringBuilder();
            builder.Append("-- ")
                .Append(parameter.ParameterName)
                .Append(": '")
                .Append((parameter.Value == null || parameter.Value == DBNull.Value) ? "null" : parameter.Value)
                .Append("' (Type = ")
                .Append(parameter.DbType);

            if (parameter.Direction != ParameterDirection.Input)
            {
                builder.Append(", Direction = ").Append(parameter.Direction);
            }
            if (!parameter.IsNullable)
            {
                builder.Append(", IsNullable = false");
            }
            if (parameter.Size != 0)
            {
                builder.Append(", Size = ").Append(parameter.Size);
            }
            if (((IDbDataParameter)parameter).Precision != 0)
            {
                builder.Append(", Precision = ").Append(((IDbDataParameter)parameter).Precision);
            }
            if (((IDbDataParameter)parameter).Scale != 0)
            {
                builder.Append(", Scale = ").Append(((IDbDataParameter)parameter).Scale);
            }

            builder.Append(")").Append(Environment.NewLine);

            return builder.ToString();
        }

    }
}