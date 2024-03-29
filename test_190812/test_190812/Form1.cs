﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_190812
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            init();
        }

        private void init()
        {
            treeView1.AllowDrop = true;
            treeView1.MouseDown += new MouseEventHandler(treeView1_mouseDown);
            treeView1.ItemDrag += new ItemDragEventHandler(TreeView1_ItemDrag);
            treeView1.DragOver += new DragEventHandler(TreeView1_DragOver);
            treeView1.DragDrop += new DragEventHandler(TreeView1_DragDrop);
        }

        private void treeView1_mouseDown(object sender , System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) { return; };
            textBox1.Text += "right_click\r\n";
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = treeView1.Nodes.Add("testNode01");
            treeNode.Nodes.Add("testChildNode0101");
        
            treeView1.Nodes[0].Nodes.Add("testChildNode0102");

            treeView1.Nodes.Add("testNode02");
            treeView1.Nodes[1].Nodes.Add("testChildNode0201");
            treeView1.Nodes.Add("testNode03");
            treeView1.Nodes[2].Nodes.Add("testChildNode0301");
            treeView1.Nodes.Add("testNode04");
            
            treeView1.ExpandAll();  

        }
        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode cln = (TreeNode)treeView1.Nodes[1].Clone();
            treeView1.Nodes[1].Remove();
            treeView1.Nodes.Insert(0,cln);
        }
        //ノードがドラッグされた時
        private void TreeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeView tv = (TreeView)sender;
            tv.SelectedNode = (TreeNode)e.Item;
            tv.Focus();
            //ノードのドラッグを開始する
            DragDropEffects dde = tv.DoDragDrop(e.Item, DragDropEffects.All);
            //移動した時は、ドラッグしたノードを削除する
            if ((dde & DragDropEffects.Move) == DragDropEffects.Move)
                tv.Nodes.Remove((TreeNode)e.Item);
        }

        //ドラッグしている時
        private void TreeView1_DragOver(object sender, DragEventArgs e)
        {
            //ドラッグされているデータがTreeNodeか調べる
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                //親ノードがクリックされたら
                if ((e.KeyState & 8) == 8 &&
                    (e.AllowedEffect & DragDropEffects.Copy) ==
                    DragDropEffects.Copy)
                    //Ctrlキーが押されていればCopy
                    //"8"はCtrlキーを表す
                    e.Effect = DragDropEffects.Copy;
                else if ((e.AllowedEffect & DragDropEffects.Move) ==
                    DragDropEffects.Move)
                {
                    //何も押されていなければMove
                    e.Effect = DragDropEffects.Move;
                }
                else
                    e.Effect = DragDropEffects.None;
            }
            else
                //TreeNodeでなければ受け入れない
                e.Effect = DragDropEffects.None;

            //マウス下のNodeを選択する
            if (e.Effect != DragDropEffects.None)
            {
                TreeView tv = (TreeView)sender;
                //マウスのあるNodeを取得する
                TreeNode target =
                    tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
                //ドラッグされているNodeを取得する
                TreeNode source =
                    (TreeNode)e.Data.GetData(typeof(TreeNode));

                //マウス下のNodeがドロップ先として適切か調べる
                if (target != null && target != source && !IsChildNode(source, target) && target.Level != 1)
                {
                    //Nodeを選択する
                    if (target.IsSelected == false)
                        tv.SelectedNode = target;
                }
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        //ドロップされたとき
        private void TreeView1_DragDrop(object sender, DragEventArgs e)
        {
            //ドロップされたデータがTreeNodeか調べる
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeView tv = (TreeView)sender;
                //ドロップされたデータ(TreeNode)を取得
                TreeNode source =
                    (TreeNode)e.Data.GetData(typeof(TreeNode));
                //ドロップ先のTreeNodeを取得する
                TreeNode target =
                    tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));

                //ドラッグしたノードが親ノードの場合
                if (source.Level == 0)
                {
                    //ドロップされたNodeのコピーを作成
                    TreeNode cln = (TreeNode)source.Clone();
                    var moveTargetIndex = 0;
                    // 上から下へのmoveかを判定する
                    if (target.Index > source.Index)
                    { moveTargetIndex = target.Index + 1; }
                    else { moveTargetIndex = target.Index; }

                    treeView1.Nodes.Insert(moveTargetIndex, cln);
                    //追加されたNodeを選択
                    tv.SelectedNode = cln;
                    treeView1.ExpandAll();
                }
                else if(source.Level == 1)
                {
                    //マウス下のNodeがドロップ先として適切か調べる
                    if (target != null && target != source &&
                        !IsChildNode(source, target))
                    {
                        //ドロップされたNodeのコピーを作成
                        TreeNode cln = (TreeNode)source.Clone();
                        //Nodeを追加
                        target.Nodes.Add(cln);
                        //ドロップ先のNodeを展開
                        target.Expand();
                        //追加されたNodeを選択
                        tv.SelectedNode = cln;
                        treeView1.ExpandAll();
                    }
                    else
                        e.Effect = DragDropEffects.None;
                }



            }
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// あるTreeNodeが別のTreeNodeの子ノードか調べる
        /// </summary>
        /// <param name="parentNode">親ノードか調べるTreeNode</param>
        /// <param name="childNode">子ノードか調べるTreeNode</param>
        /// <returns>子ノードの時はTrue</returns>
        private static bool IsChildNode(TreeNode parentNode, TreeNode childNode)
        {
            if (childNode.Parent == parentNode)
                return true;
            else if (childNode.Parent != null)
                return IsChildNode(parentNode, childNode.Parent);
            else
                return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
        }
    }
}
