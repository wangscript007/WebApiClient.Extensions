﻿using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApiClient.Contexts;

namespace WebApiClient.Attributes
{
    /// <summary>
    /// 表示Json-Rpc请求方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class JsonRpcMethodAttribute : ApiActionAttribute, IApiReturnAttribute
    {
        /// <summary>
        /// Post请求特性
        /// </summary>
        private HttpPostAttribute postAttribute = new HttpPostAttribute();

        /// <summary>
        /// 获取JsonRpc的方法名称
        /// 为null则使用声明的方法名
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// 获取或设置JsonRpc的参数风格
        /// 默认为JsonRpcParamsStyle.Array
        /// </summary>
        public JsonRpcParamsStyle ParamsStyle { get; set; } = JsonRpcParamsStyle.Array;

        /// <summary>
        /// 获取或设置提交的Content-Type
        /// 默认为application/json-rpc 
        /// </summary>
        public string ContentType { get; set; } = JsonRpc.ContentType;

        /// <summary>
        /// 获取或设置JsonRpc的路径
        /// 可以为空、相对路径或绝对路径
        /// </summary>
        public string Path
        {
            get => this.postAttribute.Path;
            set => this.postAttribute = new HttpPostAttribute(value);
        }

        /// <summary>
        /// Json-Rpc请求方法
        /// </summary>
        public JsonRpcMethodAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Json-Rpc请求方法
        /// </summary>
        /// <param name="method">JsonRpc的方法名称</param>
        public JsonRpcMethodAttribute(string method)
        {
            this.Method = method;
        }


        /// <summary>
        /// 请求前
        /// 参数特性未执行
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        public override async Task BeforeRequestAsync(ApiActionContext context)
        {
            await this.postAttribute.BeforeRequestAsync(context);
            context.RequestMessage.Content = new JsonRpcContent(context, this);
            context.RequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(this.ContentType ?? JsonRpc.ContentType));
        }

        /// <summary>
        /// 请求前
        /// 参数特性已执行完成
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        Task IApiReturnAttribute.BeforeRequestAsync(ApiActionContext context)
        {
            return JsonRpc.CompletedTask;
        }

        /// <summary>
        /// 返回请求结果
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        async Task<object> IApiReturnAttribute.GetTaskResult(ApiActionContext context)
        {
            var json = await context.ResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            var dataType = context.ApiActionDescriptor.Return.DataType.Type;
            return context.HttpApiConfig.JsonFormatter.Deserialize(json, dataType);
        }
    }
}
