using System;
using SpriteKit;
using UIKit;
using CoreGraphics;
using Foundation;

namespace Atomix
{
	public class SKButton : SKSpriteNode
	{
		bool		_enabled;
		bool		_selected;

		public static SKButton Create(string normal)
		{
			return Create(normal, null, null);
		}

		public static SKButton Create(string normal, string selected, string disabled)
		{			
			if (normal == null)
				throw new ArgumentException("normal");
			if (selected == null)
				selected = normal + "+Selected";

			SKTexture normalTexture, selectedTexture, disabledTexture;

			normalTexture 	= Atlases.Buttons.Get(normal);
			selectedTexture = Atlases.Buttons.Get(selected);
			disabledTexture = Atlases.Buttons.Get(disabled);

			return new SKButton(normalTexture, selectedTexture, disabledTexture);
		}

		public SKButton(SKTexture normal, SKTexture selected, SKTexture disabled = null) : base(normal)
		{
			if (normal == null)
				throw new ArgumentNullException("normal");

			Initialize(normal, selected, disabled);
		}

		public SKButton(UIColor color, CGSize size) : base(color, size) 
		{
			Initialize(null, null, null);
		}

		public SKButton(SKTexture texture, UIColor color, CGSize size) : base(texture, color, size) 
		{
			Initialize(texture, null, null);
		}

		public SKButton(IntPtr handle) : base(handle) { }

		void Initialize(SKTexture normal, SKTexture selected, SKTexture disabled)
		{
			NormalTexture 	= normal;
			SelectedTexture = selected;
			DisabledTexture = disabled;
			Enabled 		= true;
			Selected 		= false;
			ZPosition  		= Constants.ButtonZIndex;
			UserInteractionEnabled = true;
		}

		public SKTexture SelectedTexture	{ get; set; }
		public SKTexture DisabledTexture	{ get; set; }

		public event EventHandler Clicked ;

		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (_enabled == value)
					return; // No Change

				_enabled = value;
				if (value)
				{
					if (Selected)
						this.Texture = SelectedTexture ?? NormalTexture;
					else
						this.Texture = NormalTexture;
				}
				else
				{
					this.Texture = DisabledTexture ?? NormalTexture;
				}
			}
		} 

		public bool Selected
		{
			get { return _selected && Enabled ; }
			set
			{
				if (_selected == value)
					return ; // No Change

				_selected = value;

				if (Selected)
					this.Texture = SelectedTexture ?? NormalTexture;
				else if (! Enabled)
					this.Texture = DisabledTexture ?? NormalTexture;
				else
					this.Texture = NormalTexture;
			}
		}

		bool IsSelectTouch(NSSet touches)
		{
			bool selected = false;

			if (Enabled)
			{
				foreach(UITouch touch in touches)
				{
					CGPoint touchPoint = touch.LocationInNode(this.Parent);
					selected = this.Frame.Contains(touchPoint);
				}

				this.Selected = selected;
			}

			return selected;
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			this.Selected = IsSelectTouch(touches);
			base.TouchesBegan (touches, evt);
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			this.Selected = IsSelectTouch(touches);
			base.TouchesMoved(touches, evt);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{			
			base.TouchesEnded(touches, evt);

			Selected = false;

			if (Enabled)
			{
				var handle = Clicked;

				if (handle != null)
				{
					bool isSelect = IsSelectTouch(touches);
					if (isSelect)
						handle(this, EventArgs.Empty);
				}
			}
		}

		public override void TouchesCancelled (Foundation.NSSet touches, UIEvent evt)
		{
			this.Selected = false;
			base.TouchesCancelled (touches, evt);
		}
	}
}
