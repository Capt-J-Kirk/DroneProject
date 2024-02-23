

1. put both scripts, userInput and the controller script on same GameObject

2. in the controller script, make sure theire is a public functions that takes the input.

3. in the userInput script, call getComponent<"controller script here">()."that public function"("value to pass");




Other methods, if only one instance of the userInput exist in the game!



in the userInput script, add
- public static userInput GCInstance;
- void Awake(){GCInstance = this;}

in the controller script, two methods, can be called in either  Update or fixedUpdate! 

- userInput.GCInstance."parameter value that should be passed"

