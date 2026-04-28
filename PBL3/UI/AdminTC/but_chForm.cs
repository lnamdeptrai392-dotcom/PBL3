using System.Windows.Controls;
using System.Windows.Media;

namespace PBL3a.UI.AdminTC
{
    public class but_chform
    {
        public static Button currentButton;
        public static UserControl childform;

        public static void ActivateButton(object sender)
        {
            if (sender != null && currentButton != (Button)sender)
            {
                DisableButton();

                currentButton = (Button)sender;
                currentButton.Background = new SolidColorBrush(Color.FromRgb(144, 188, 245));
            }
        }

        public static void Activate_DisableButton_fast(object sender)
        {
            if (sender != null)
            {
                currentButton = (Button)sender;
                currentButton.Background = new SolidColorBrush(Color.FromRgb(144, 188, 245));

                DisableButton();
            }
        }

        public static void DisableButton()
        {
            if (currentButton != null)
            {
                currentButton.Background = new SolidColorBrush(Color.FromRgb(44, 78, 98));
            }
        }

        public static void OpenChildForm(UserControl child, object sender, ContentControl paDesktop)
        {
            ActivateButton(sender);

            childform = child;
            paDesktop.Content = childform;
        }

        public static void bt_MouseEnter(object sender)
        {
            Button bt = (Button)sender;
            bt.Background = new SolidColorBrush(Color.FromRgb(44, 78, 98));
        }

        public static void bt_MouseLeave(object sender)
        {
            Button bt = (Button)sender;
            bt.Background = new SolidColorBrush(Color.FromRgb(44, 78, 98));
        }
    }
}