using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Threading;

namespace ElectronicObserver.Window.Plugins
{
    public class PluginManager
    {
        FormMain Main = null;
        public Dictionary<string, ElectronicPlugin> Plugins
        {
            get;
            set;
        }

        public PluginManager(FormMain main)
        {
            Main = main;
            Plugins = new Dictionary<string, ElectronicPlugin>();
        }

        public bool AddPlugin(ElectronicPlugin Plugin)
        {
            Plugins[Plugin.PluginName] = Plugin;
            if (Plugin is IDialogPlugin)
            {
                IDialogPlugin plugin = Plugin as IDialogPlugin;
                var item = new ToolStripMenuItem
                {
                    Name = "ToolMenuItem_" + Plugin.PluginName,
                    Text = plugin.ToolMenuTitle,
                    Tag = plugin
                };
                if (Plugin.MenuIcon != null)
                    item.Image = Plugin.MenuIcon;
                item.Click += dialogPlugin_Click;
                ((ToolStripMenuItem)(Main.MainMenuStrip.Items["StripMenu_Tool"])).DropDownItems.Add(item);
            }
            if (Plugin is IDockPlugin)
            {
                IDockPlugin plugin = Plugin as IDockPlugin;
                DockContent dockContent = plugin.GetDockWindow() as DockContent;
                dockContent.HideOnClose = true;
                dockContent.Name = "DockWindow_" + Plugin.PluginName;
                var item = new ToolStripMenuItem
                {
                    Name = "ViewMenuItem_" + Plugin.PluginName,
                    Text = plugin.ViewMenuTitle,
                    Tag = dockContent
                };
                if (Plugin.MenuIcon != null)
                    item.Image = Plugin.MenuIcon;
                item.Click += menuitem_Click;
                ((ToolStripMenuItem)(Main.MainMenuStrip.Items["StripMenu_View"])).DropDownItems.Add(item);

                Main.SubForms.Add(dockContent);
            }
            bool b = Plugin.StartPlugin(Main);
            if (b)
                Utility.Logger.Add(2, string.Format("插件 {0}({1}) 已加载。", Plugin.PluginName, Plugin.Version));
            else
                Utility.Logger.Add(2, string.Format("插件 {0}({1}) 加载失败。", Plugin.PluginName, Plugin.Version));
            return b;

        }

        void dialogPlugin_Click(object sender, EventArgs e)
        {
            var plugin = (IDialogPlugin)((ToolStripMenuItem)sender).Tag;
            if (plugin != null)
            {
                try
                {
                    plugin.GetToolWindow().Show(Main);
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    Utility.ErrorReporter.SendErrorReport(ex, string.Format("插件显示出错：{0}", plugin.ToolMenuTitle));
                }
            }
        }

        void menuitem_Click(object sender, EventArgs e)
        {
            var f = ((ToolStripMenuItem)sender).Tag as DockContent;
            if (f != null)
            {
                f.Show(Main.MainPanel);
            }
        }

    }

    [Obsolete]
    public class PluginManagerObsolete
    {
        FormMain Main = null;
        public Dictionary<string, ElectronicPluginContainer> PluginContainers
        {
            get;
            set;
        }

        public PluginManagerObsolete(FormMain main)
        {
            Main = main;
            PluginContainers = new Dictionary<string, ElectronicPluginContainer>();
        }

        public bool StartPlugin(string name)
        {
            if (PluginContainers.ContainsKey(name))
            {
                if (!PluginContainers[name].Plugin.Active)
                {
                    var Plugin = PluginContainers[name].Plugin;
                    if (Plugin is IDialogPlugin)
                    {
                        IDialogPlugin plugin = Plugin as IDialogPlugin;
                        var item = new ToolStripMenuItem
                        {
                            Name = "ToolMenuItem_" + Plugin.PluginName,
                            Text = plugin.ToolMenuTitle,
                            Tag = plugin
                        };
                        if (Plugin.MenuIcon != null)
                            item.Image = Plugin.MenuIcon;
                        item.Click += dialogPlugin_Click;
                        ((ToolStripMenuItem)(Main.MainMenuStrip.Items["StripMenu_Tool"])).DropDownItems.Add(item);
                    }
                    if (Plugin is IDockPlugin)
                    {
                        IDockPlugin plugin = Plugin as IDockPlugin;
                        DockContent dockContent = plugin.GetDockWindow() as DockContent;
                        dockContent.HideOnClose = true;
                        dockContent.Name = "DockWindow_" + Plugin.PluginName;
                        var item = new ToolStripMenuItem
                        {
                            Name = "ViewMenuItem_" + Plugin.PluginName,
                            Text = plugin.ViewMenuTitle,
                            Tag = dockContent
                        };
                        if (Plugin.MenuIcon != null)
                            item.Image = Plugin.MenuIcon;
                        item.Click += menuitem_Click;
                        ((ToolStripMenuItem)(Main.MainMenuStrip.Items["StripMenu_View"])).DropDownItems.Add(item);

                        Main.SubForms.Add(dockContent);
                    }
                    Plugin.Active = true;
                    bool b = Plugin.StartPlugin(Main);
                    if (b)
                        Utility.Logger.Add(2, string.Format("插件 {0}({1}) 已启动。", Plugin.PluginName, Plugin.Version));
                    else
                        Utility.Logger.Add(2, string.Format("插件 {0}({1}) 启动失败。", Plugin.PluginName, Plugin.Version));
                    return b;
                }
            }
            return false;
        }

        void dialogPlugin_Click(object sender, EventArgs e)
        {
            var plugin = (IDialogPlugin)((ToolStripMenuItem)sender).Tag;
            if (plugin != null)
            {
                try
                {
                    plugin.GetToolWindow().Show(Main);
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    Utility.ErrorReporter.SendErrorReport(ex, string.Format("插件显示出错：{0}", plugin.ToolMenuTitle));
                }
            }
        }

        void menuitem_Click(object sender, EventArgs e)
        {
            var f = ((ToolStripMenuItem)sender).Tag as DockContent;
            if (f != null)
            {
                f.Show(Main.MainPanel);
            }
        }

        public bool StopPlugin(string name)
        {
            if (PluginContainers.ContainsKey(name))
            {
                if (PluginContainers[name].Plugin.Active)
                {
                    var Plugin = PluginContainers[name].Plugin;
                    if (Plugin is IDialogPlugin)
                    {
                        IDialogPlugin plugin = Plugin as IDialogPlugin;

                        string ItemName = "ToolMenuItem_" + Plugin.PluginName;
                        ((ToolStripMenuItem)(Main.MainMenuStrip.Items["StripMenu_Tool"])).DropDownItems.RemoveByKey(ItemName);
                    }
                    if (Plugin is IDockPlugin)
                    {
                        IDockPlugin plugin = Plugin as IDockPlugin;

                        string ItemName = "ViewMenuItem_" + Plugin.PluginName;
                        ((ToolStripMenuItem)(Main.MainMenuStrip.Items["StripMenu_View"])).DropDownItems.RemoveByKey(ItemName);
                        Main.SubForms.ForEach(e => { if (e.Name == "DockWindow_" + Plugin.PluginName) e.Close(); });
                        Main.SubForms.RemoveAll(e => e.Name == "DockWindow_" + Plugin.PluginName);
                    }
                    Plugin.Active = false;
                    bool b = Plugin.StopPlugin();
                    if (b)
                        Utility.Logger.Add(2, string.Format("插件 {0}({1}) 已停止。", Plugin.PluginName, Plugin.Version));
                    else
                        Utility.Logger.Add(2, string.Format("插件 {0}({1}) 停止失败。", Plugin.PluginName, Plugin.Version));
                    return b;
                }
            }
            return false;
        }
        public bool LoadPlugin(string dllPath)
        {
            var oldplugin = PluginContainers.Values.FirstOrDefault(e => e.PluginPath == dllPath);
            if (oldplugin != null)
                UnloadPlugin(oldplugin.Plugin.PluginName);

            ElectronicPluginContainer container = new ElectronicPluginContainer();
            var plugin = container.LoadPlugin(dllPath);
            if (plugin == null)
                return false;
            string pluginName = plugin.PluginName;

            PluginContainers[pluginName] = container;
            Utility.Logger.Add(2, string.Format("插件 {0}({1}) 已成功加载。", plugin.PluginName, plugin.Version));
            return true;
        }

        public bool UnloadPlugin(string name)
        {
            if (PluginContainers.ContainsKey(name))
            {
                StopPlugin(name);

                AppDomain.Unload(PluginContainers[name].appDomain);
                Utility.Logger.Add(2, string.Format("插件 {0}({1}) 已成功卸载。", PluginContainers[name].Plugin.PluginName, PluginContainers[name].Plugin.Version));
                PluginContainers.Remove(name);
                return true;
            }
            return false;
        }


    }
    [Obsolete]
    public class ElectronicPluginContainer
    {
        public ElectronicPlugin Plugin
        {
            get;
            set;
        }
        public string DomainName
        {
            get;
            set;
        }

        public AppDomain appDomain
        {
            get;
            set;
        }

        public string PluginPath
        {
            get;
            set;
        }

        public ElectronicPlugin LoadPlugin(string dllPath)
        {
            DomainName = Path.GetFileNameWithoutExtension(dllPath);
            appDomain = AppDomain.CreateDomain(DomainName);

            string name = Path.GetFileName(Application.ExecutablePath);
            ElectronicPluginFactory factory = (ElectronicPluginFactory)appDomain.CreateInstanceFromAndUnwrap(name, typeof(ElectronicPluginFactory).FullName);
            factory.LoadAssembly(dllPath);
            Plugin = factory.Invoke();

            if (Plugin == null)
            {
                AppDomain.Unload(appDomain);
                return null;
            }
            appDomain.UnhandledException += appDomain_UnhandledException;
            return Plugin;
        }

        void appDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            MessageBox.Show(ex.ToString(), "ElectronicObserver", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Utility.ErrorReporter.SendErrorReport(ex, DomainName + "插件中错误：" + ex.Message);
        }
    }

    [Obsolete]
    class ElectronicPluginFactory : MarshalByRefObject
    {
        Assembly assembly = null;

        public void LoadAssembly(string dll)
        {
            string path = Application.StartupPath;
            assembly = Assembly.LoadFile(dll);
        }
        public ElectronicPlugin Invoke()
        {
            if (assembly == null)
                return null;
            var types = assembly.GetExportedTypes();
            foreach (var Type in types)
            {
                if (Type.IsSubclassOf(typeof(ElectronicPlugin)))
                {
                    var plugin = assembly.CreateInstance(Type.FullName) as ElectronicPlugin;
                    return plugin;
                }
            }
            return null;
        }
    }
}
