package com.game.bizModules.move.msg;

import com.game.gameServer.msg.AbstractGameMsg;

/**
 * 移动消息
 * 
 * @author hjj2017
 *
 */
public class CGMoveTo extends AbstractGameMsg {
	/** 消息类型 ID */
	private static final short MSG_TYPE_ID = 1002;

	/** 目标位置 X */
	public int _x = -1;
	/** 目标位置 Y */
	public int _y = -1;

	@Override
	public short getMsgTypeId() {
		return MSG_TYPE_ID;
	}
}