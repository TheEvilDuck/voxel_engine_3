using System;

public class InputCharacterMediator: IDisposable
{
    private PlayerInput _playerInput;
    private Character _character;

    public InputCharacterMediator(PlayerInput playerInput, Character character)
    {
        _playerInput = playerInput;
        _character = character;

        _playerInput.movementChanged+=_character.Move;
    }

    public void Dispose()
    {
         _playerInput.movementChanged-=_character.Move;
    }
}
