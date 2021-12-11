using Microsoft.Win32;
using System;
using System.Collections.Generic;
using Walterlv.Win32;
using System.Security.Principal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtOpenFunction
{
    /// <summary>
    /// 注册表中文件扩展名的结构体
    /// </summary>
    public struct FileExtStruct
    {

    }
    /// <summary>
    /// 向注册表中注册文件扩展名
    /// </summary>
    class RegisterFileExtension
    {
        /// <summary>
        /// 文件后缀名（带.）
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        /// 该后缀名所指示的文件的类型
        /// e.g. text/plain
        /// [MIME 类型 - HTTP | MDN](https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Basics_of_HTTP/MIME_types )
        /// [Media Types](https://www.iana.org/assignments/media-types/media-types.xhtml )
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 该后缀名所指示的文件的感知类型
        /// e.g. text
        /// [Perceived Types (Windows) | Microsoft Docs](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/cc144150(v%3Dvs.85) )
        /// </summary>
        public string PerceivedType { get; set; }

        /// <summary>
        /// 该后缀名所指示的文件关联的默认应用程序的 ProgramId
        /// </summary>
        public string DefaultProgramId { get; set; }

        /// <summary>
        /// 该后缀名所指示的文件，还可以被哪些 ProgramId 所代表的程序打开。
        /// </summary>
        public IList<string> OpenWithProgramIds { get; set; } = new List<string>();

        /// <summary>
        /// 根据指定文件扩展名，创建 <see cref="RegisterFileExtension"/> 的实例。
        /// </summary>
        /// <param name="fileExtension"></param>
        public RegisterFileExtension(string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new ArgumentNullException(nameof(fileExtension));
            }

            if (!fileExtension.StartsWith(".", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"{fileExtension} is not a valid file extension. it must start with \".\"",
                    nameof(fileExtension));
            }
            FileExtension = fileExtension;
        }

        /// <summary>
        /// 将此文件扩展名注册到当前用户的注册表中
        /// </summary>
        public void WriteToCurrentUser()
        {
            WriteToRegistry(RegistryHive.CurrentUser);
        }

        /// <summary>
        /// 将此文件扩展名注册到所有用户的注册表中。（进程需要以管理员身份运行）
        /// </summary>
        public void WriteToAllUser()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                WriteToRegistry(RegistryHive.LocalMachine);
            }
            else
            {
                throw new Exception("Is not administrator state.");
            }
        }

        /// <summary>
        /// 将此文件扩展名写入到注册表中
        /// </summary>
        private void WriteToRegistry(RegistryHive registryHive)
        {
            // 写默认执行程序
            registryHive.Write32(BuildRegistryPath(FileExtension), DefaultProgramId ?? string.Empty);

            // 写 ContentType
            if (ContentType != null && !string.IsNullOrWhiteSpace(ContentType))
            {
                registryHive.Write32(BuildRegistryPath(FileExtension), "Content Type", ContentType);
            }

            // 写 PerceivedType
            if (PerceivedType != null && !string.IsNullOrWhiteSpace(PerceivedType))
            {
                registryHive.Write32(BuildRegistryPath(FileExtension), "PerceivedType", PerceivedType);
            }

            // 写 OpenWithProgramIds
            if (OpenWithProgramIds.Count > 0)
            {
                foreach (string programId in OpenWithProgramIds)
                {
                    registryHive.Write32(BuildRegistryPath($"{FileExtension}\\OpenWithProgids"), programId, string.Empty);
                }
            }
        }

        private string BuildRegistryPath(string relativePath)
        {
            return $"Software\\Classes\\{relativePath}";
        }

    }
}
