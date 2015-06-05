using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FrozenIsigniaClasses;

namespace FrozenIsignia
{
    public class Options : NetworkControl
    {
        private int selection = 0;
        private Font titleFont = new Font("Arial", 36);
        private Font selectionFont = new Font("Arial", 16);
        private String name = Properties.Settings.Default.Name;
        private bool editingName = false;
        private bool suppress = true;
        private String[] options;

        public Options(NetworkHandler network) : base(network)
        {
            updateOptions();
        }

        public override void receive(string[] msg) { }

        private void updateOptions()
        {
            options = new String[]
            {
                "Name: " + name,
                "Squire: " + Properties.Settings.Default.Squire,
                "FullScreen: " + Properties.Settings.Default.FullScreen,
                "Exit"
            };

            Invalidate();
        }

        private void editName(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 8 && name.Length > 0)
                name = name.Substring(0, name.Length - 1);
            else if (e.KeyChar == 13)
            {
                if (suppress)
                    suppress = false;
                else
                {
                    editingName = false;
                    suppress = true;
                    this.KeyPress -= editName;
                    Properties.Settings.Default.Name = name;
                    Properties.Settings.Default.Save();
                    network.send("NAME " + name);
                }
            }
            else if (char.IsLetterOrDigit(e.KeyChar))
                name += e.KeyChar;

            updateOptions();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!editingName)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        replaceControl(new Title(network));
                        break;
                    case Keys.W:
                    case Keys.Up:
                        if (--selection < 0)
                            selection = options.Length - 1;
                        break;
                    case Keys.S:
                    case Keys.Down:
                        selection = (selection + 1) % options.Length;
                        break;
                    case Keys.Enter:
                        if (selection == 0)
                        {
                            this.KeyPress += editName;
                            editingName = true;
                        }
                        else if(selection == 1)
                        {
                            for (int i = 0; i < Unit.squires.Length; i++)
                            {
                                if (Unit.squires[i] == Properties.Settings.Default.Squire)
                                {
                                    Properties.Settings.Default.Squire = Unit.squires[(i + 1) % Unit.squires.Length];
                                    Properties.Settings.Default.Save();
                                    updateOptions();
                                    break;
                                }
                            }
                        }
                        else if(selection == 2)
                        {
                            MainForm form = (MainForm)FindForm();
                            form.setFullscreen(!Properties.Settings.Default.FullScreen);
                            Properties.Settings.Default.Save();
                            updateOptions();
                        }
                        else if (selection == 3)
                            replaceControl(new Title(network));
                        break;
                }

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(System.Drawing.Color.FromArgb(20, 20, 20));

            StringFormat align = new StringFormat();
            align.Alignment = StringAlignment.Center;
            align.LineAlignment = StringAlignment.Far;

            g.DrawString("Options", titleFont, Brushes.White, new RectangleF(0, 0, ClientSize.Width, 0.3f * ClientSize.Height), align);

            String selectString = "";
            for (int i = 0; i < options.Length; i++)
            {
                if (i == selection)
                    selectString += "> ";
                selectString += options[i];
                if (i == selection)
                    selectString += " <";
                if (i != options.Length - 1)
                    selectString += "\n";
            }
            align.LineAlignment = StringAlignment.Near;
            g.DrawString(selectString, selectionFont, Brushes.White, new RectangleF(0, 0.3f * ClientSize.Height, ClientSize.Width, 0.7f * ClientSize.Height), align);
        }
    }
}
