//TODO: format this

Setup Instructions
1. Attach the BetterTyping prefab to some asset.
	- Drag the BetterTyping asset in the <path> folder onto the <character asset>.
2. Setup objects that pullup the BetterTyping Daisy Wheel Menu
	- Create an interaction and pass in the InputAction that is being used to recieve controller inputs.


Costomization Instructions

Changing the number of options in the BetterTyping Daisy Wheel Menu
	- The number of options is determined by the number of TAB-separated elements in each row. Addin another TAB-separated element will change the number of options



BTSetupDW			//purely cosmetic. sets up DW
	position the UI elems
	calc angles for each DW slice
	setup text

BTInteractions		//handles IO while the BT DW is active
	update current DW slice
	cycle DWs
	handle input
	exit BT and resume previouse ctrl scheme